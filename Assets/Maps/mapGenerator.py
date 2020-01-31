r"""
d:
cd D:\Users\Andrei\Conquest\Conquest\Assets\Maps
python mapGenerator.py
"""

import json
from json import JSONEncoder
import random

class BoardSpace:
	boardPosition = []
	anchorHeight = None
	spriteName = None

class Board:
	spaces = []
	rows = None
	columns = None

SPRITE_NAME_POOL = ["grass", "grass", "tile"]
HEIGHT_POOL = [0.0, 0.5]
def generateBoard(rows: int, columns: int):
	board = Board()
	board.rows = rows
	board.columns = columns
	spaces = []
	for i in range(0, columns):
		for j in range(0, rows):
			space = BoardSpace()
			space.boardPosition = [i, j]
			space.anchorHeight = random.choice(HEIGHT_POOL)
			space.spriteName = random.choice(SPRITE_NAME_POOL)
			spaces.append(space.__dict__)
	board.spaces = spaces
	board = board.__dict__
	with open('mapData.txt', 'w') as file:
		json.dump(obj = board, fp = file, indent = 3)
		file.close()

generateBoard(10, 10)