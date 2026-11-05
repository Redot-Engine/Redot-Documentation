import importlib.util
import json
from pathlib import Path, PureWindowsPath
from types import SimpleNamespace
import unittest
from unittest.mock import patch


spec = importlib.util.spec_from_file_location("rendering_docs", Path(__file__).parents[1] / "rendering_docs.py")
docs = importlib.util.module_from_spec(spec)
spec.loader.exec_module(docs)


class SourceMarkerTests(unittest.TestCase):
    def test_comments_and_explicit_diagnostics_keep_original_line_numbers(self):
        source = '\n'.join([
            'int todo = count; if (todo) { todo--; }',
            'const char *label = "TODO // FIXME";',
            'const char *url = "https://example.test"; // TODO trailing comment',
            'int value; ///< @todo document this member',
            '/* comment starts',
            ' * FIXME unfinished path',
            ' */ int hack = 0;',
            'ERR_FAIL_MSG("Not yet implemented");',
            '// HACK temporary workaround',
            '/* TODO first */ int todo; // FIXME second',
            'DEV_ASSERT(false && "FIXME: Broken.");',
            'ERR_FAIL_MSG("TODO: implement this operation");',
            'ERR_FAIL_COND(todo);',
        ])
        self.assertEqual([3, 4, 6, 8, 9, 10, 11, 12], [m['line'] for m in docs.source_markers(source)])

    def test_literals_do_not_open_comments(self):
        source = '\n'.join([
            r'const char *escaped = "quote: \" // TODO";',
            'auto raw = u8R"tag(// TODO',
            '/* FIXME */)tag";',
            'auto character = \'"\'; // TODO real comment',
            "int count = 1'000; // FIXME digit separator",
            '// continued comment \\',
            'TODO on continued line',
            '/* unfinished comment HACK',
        ])
        self.assertEqual([4, 5, 7, 8], [m['line'] for m in docs.source_markers(source)])


class SourceLocatorTests(unittest.TestCase):
    def test_definition_specific_locator_skips_forward_declaration_and_related_type(self):
        source = 'class DependencyTracker;\nclass DependencyTracker {\n};\n'
        self.assertEqual(2, docs.locate_source(source, 'utilities.h', 'class DependencyTracker {'))
        metal = 'enum class MDCommandBufferStateType {};\nclass API_AVAILABLE(macos(11.0)) MDCommandBuffer {\n};'
        self.assertEqual(2, docs.locate_source(metal, 'metal_objects.h', ') MDCommandBuffer {'))

    def test_missing_empty_and_ambiguous_locators_fail(self):
        for locator, message in [('', 'Empty'), ('missing', 'Missing'), ('class Item', 'Ambiguous')]:
            with self.subTest(locator=locator), self.assertRaisesRegex(ValueError, message):
                docs.locate_source('class Item;\nclass Item {};', 'item.h', locator)


class GitPathTests(unittest.TestCase):
    def test_git_paths_remain_posix_even_with_windows_host_path_class(self):
        files = {
            'servers/rendering/example.h': '#include "detail/helper.h"\nclass Example {};\n',
            'servers/rendering/example.cpp': '#include "example.h"\n',
            'servers/rendering/detail/helper.h': 'struct Helper {};\n',
        }
        def git(_engine, command, *args):
            if command == 'ls-tree':
                return '\n'.join(files).encode()
            self.assertEqual('show', command)
            return files[args[0].split(':', 1)[1]].encode()

        snapshot = dict(source_revision='revision', reviewed_revision='revision',
                        reviewed_on='2026-11-04', engine_version='26.4 alpha',
                        repository='https://github.com/Redot-Engine/redot-engine')
        with patch.object(docs, 'git', side_effect=git), \
             patch.object(docs.subprocess, 'run', return_value=SimpleNamespace(returncode=0, stdout='')), \
             patch.object(docs, 'ANCHORS') as anchors, patch.object(docs, 'Path', PureWindowsPath):
            anchors.read_text.return_value = '[]'
            outputs = docs.build('engine', snapshot)
        inventory = json.loads(outputs['rendering_inventory.json'])['primary']
        header = next(item for item in inventory if item['path'].endswith('example.h'))
        self.assertEqual('servers/rendering/detail/helper.h', header['includes'][0]['resolved'])
        implementation = next(item for item in inventory if item['path'].endswith('.cpp'))
        self.assertEqual('servers/rendering/example.h', implementation['includes'][0]['resolved'])
        self.assertIn('servers/rendering/example.cpp#L1', outputs['rendering_classes_core.md'])
        self.assertIn('doc_rendering_classes_core', outputs['index.md'])


if __name__ == '__main__':
    unittest.main()
