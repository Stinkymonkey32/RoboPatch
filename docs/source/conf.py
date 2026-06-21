import os
import sys

sys.path.insert(0, os.path.abspath('..'))

project = 'roboPatch'
author = 'roboPatch contributors'
release = '0.1.0'

extensions = [
    'sphinx.ext.autodoc',
    'sphinx.ext.napoleon',
    'sphinx.ext.viewcode',
    'sphinx.ext.githubpages',
    'myst_parser',
]

templates_path = ['_templates']
exclude_patterns = []

html_theme = 'alabaster'
html_static_path = ['_static']

autodoc_typehints = "description"