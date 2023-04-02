import os
"""
path = "C://Users//geonh//Documents//Godot//Henry's Caravan//Sprites//Effects//purple"
file_list = os.listdir(path)

for i in file_list:
    with open(os.path.join(path, i),'r') as file:
        filedata = file.read()
        for x in range(164):
            filedata = filedata.replace('blue.', 'purple.', 1)
    with open(os.path.join(path, i),'w') as file:
        file.write(filedata)
"""


i = 16
with open("temp.txt",'r') as file:
    filedata = file.read()
    for x in range(90):
        if i % 15 >= 12 or i % 15 == 0:
            filedata = filedata.replace('!', str(i), 3)
        else:
            filedata = filedata.replace('!', str(i), 1)
        i += 1
with open("temp.txt",'w') as file:
    file.write(filedata)
