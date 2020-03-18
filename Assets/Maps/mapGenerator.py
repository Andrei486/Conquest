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
	players = []

class PlayerInfo:
	shorthand = False
	name = None
	affiliation = None
	boardPosition = []
	attackPower = 1.0
	defense = 1.0
	jumpHeight = 1.0
	maxHealth = 10.0
	currentHealth = 10.0
	level = 1
	moveRange = 3
	maxActions = 3
	maxBullets = 3
	skillNames = []

class ShorthandPlayer:
	shorthand = True
	name = None
	affiliation = None
	boardPosition = []
	currentHealth = 10.0
	level = 1

SPRITE_NAME_POOL = ["grass", "grass", "tile"]
HEIGHT_POOL = [0.0, 0.5]
def generateBoard(rows: int, columns: int) -> Board:
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
	return board

def addUnitPlaceholders(board: Board, count: int) -> Board:
	players = []
	for i in range(count):
		player = PlayerInfo()
		player.name = "Unit " + str(i)
		player.affiliation = "PLAYER"
		player.boardPosition = board.spaces[i]["boardPosition"]
		player.maxHealth = 10.0
		player.currentHealth = 10.0
		player.level = 3
		player.attackPower = 5.0
		player.defense = 3.0
		player.jumpHeight = 1.0
		player.moveRange = 3
		player.maxActions = 3
		player.maxBullets = 3
		player.skillNames = ["Full Moon", "Crescent Slash", "Artillery"]
		player = player.__dict__
		players.append(player)
	board.players = players
	return board
	
def toJSON(board: Board) -> None:
	board = board.__dict__
	with open('mapData.json', 'w') as file:
		json.dump(obj = board, fp = file, indent = 4)
		file.close()

toJSON(addUnitPlaceholders(generateBoard(30, 30), 3))