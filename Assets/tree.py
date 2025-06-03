#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Script pour lister uniquement la hiérarchie des dossiers à partir du répertoire courant
et écrire le résultat dans un fichier texte.
"""

import os
import sys

def write_dirs(path, prefix, file_handle):
    """
    Parcourt récursivement les sous-dossiers de `path` et écrit leur structure
    avec préfixe `prefix` dans le fichier `file_handle`.
    """
    try:
        entries = sorted(os.listdir(path))
    except PermissionError:
        file_handle.write(prefix + "└── [Permission refusée]\n")
        return

    # Ne garder que les dossiers
    dirs = [name for name in entries if os.path.isdir(os.path.join(path, name))]

    for index, name in enumerate(dirs):
        connector = "└── " if index == len(dirs) - 1 else "├── "
        file_handle.write(prefix + connector + name + "\n")
        # Choisir le préfixe pour les niveaux suivants
        extension = "    " if index == len(dirs) - 1 else "│   "
        write_dirs(os.path.join(path, name), prefix + extension, file_handle)


def main():
    # Chemin de départ (paramètre optionnel)
    start_path = sys.argv[1] if len(sys.argv) > 1 else os.getcwd()
    # Nom du fichier de sortie (paramètre optionnel)
    output_file = sys.argv[2] if len(sys.argv) > 2 else "hierarchie_dossiers.txt"

    with open(output_file, "w", encoding="utf-8") as f:
        f.write(start_path + "\n")
        write_dirs(start_path, "", f)

    print(f"Hiérarchie des dossiers écrite dans '{output_file}'")


if __name__ == "__main__":
    main()