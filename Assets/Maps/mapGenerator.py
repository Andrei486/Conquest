r"""
d:
cd D:\Users\Andrei\Conquest\Conquest\Assets\Maps
python mapGenerator.py
"""

import json
from json import JSONEncoder
import random

class Vector2:
	x = 0
	y = 0

	def __init__(self, x, y):
		self.x = x
		self.y = y

class Vector3:
	x = 0
	y = 0
	z = 0

	def __init__(self, x, y, z):
		self.x = x
		self.y = y
		self.z = z

class Color:
	r = 0
	g = 0
	b = 0
	a = 0

	def __init__(self, r, g, b, a):
		self.r = r
		self.g = g
		self.b = b
		self.a = a

class BoardSpace:
	boardPosition = None
	anchorPosition = None
	spriteName = None

	def __init__(self, i, j, z, spriteName):
		self.boardPosition = Vector2(i, j).__dict__
		self.anchorPosition = Vector3(i * BOARD_SIZE, z, j * BOARD_SIZE).__dict__
		self.spriteName = spriteName

class Board:
	spaces = []
	rows = None
	columns = None
	units = []
	pillarColor = None

	def __init__(self, rows, columns, spaces):
		self.rows = rows
		self.columns = columns
		self.spaces = spaces

	def setAesthetics(self, color):
		self.pillarColor = color.__dict__

class PlayerInfo:
	name = None
	affiliation = None
	boardPosition = None
	attackPower = 1.0
	defense = 1.0
	jumpHeight = 1.0
	maxHealth = 10.0
	currentHealth = 10.0
	level = 1
	moveRange = 3
	maxActions = 3
	maxBullets = 3
	bullets = 3
	turnEnded = False
	skillNames = []

	def __init__(self, name, affiliation):
		self.name = name
		self.affiliation = affiliation
	
	def setPosition(self, space):
		self.boardPosition = space["boardPosition"]


SPRITE_NAME_POOL = ["grass", "grass", "tile"]
SKILL_NAME_POOL = ["Full Moon", "Crescent Slash", "Artillery", "Sniper"]
HEIGHT_POOL = [0.0, 0.5]
BOARD_SIZE = 2.0

def generateBoard(rows: int, columns: int) -> Board:
	spaces = []
	for i in range(0, columns):
		for j in range(0, rows):
			anchorHeight = random.choice(HEIGHT_POOL)
			spriteName = random.choice(SPRITE_NAME_POOL)
			space = BoardSpace(i, j, anchorHeight, spriteName)
			spaces.append(space.__dict__)
	return Board(rows, columns, spaces)

def addUnitPlaceholders(board: Board, count: int) -> Board:
	players = []
	for i in range(count):
		player = PlayerInfo("Unit " + str(i), "PLAYER")
		player.setPosition(board.spaces[i])
		player.maxHealth = 10.0
		player.currentHealth = 10.0
		player.level = 3
		player.attackPower = 5.0
		player.defense = 3.0
		player.jumpHeight = 1.0
		player.moveRange = 3
		player.maxActions = 3
		player.maxBullets = 3
		player.bullets = 3
		player.turnEnded = False
		player.skillNames = random.sample(SKILL_NAME_POOL, 3)
		player = player.__dict__
		players.append(player)
	board.units = players
	return board
	
def toJSON(board: Board) -> None:
	board = board.__dict__
	with open('mapData.json', 'w') as file:
		json.dump(obj = board, fp = file, indent = 4)
		file.close()


board = addUnitPlaceholders(generateBoard(30, 30), 3)
board.setAesthetics(Color(0.22745, 0.15686, 0.12549, 1.0))
toJSON(board)