# script for creating sensorthings fgdb - Featureclass st_locations
# todo check path GDB_LOCATION
# run with: propy initdb.py
import arcpy
import os

# use parameters?
# GDB_Location = arcpy.GetParameterAsText(0)
# GDB_name = arcpy.GetParameterAsText(1)
GDB_Location = "D:/gisdata/arcsensor"
GDB_Name = "arcsensor.gdb"
gdb = os.path.join(GDB_Location, GDB_Name)
if not os.path.isdir(gdb):
	arcpy.CreateFileGDB_management(GDB_Location, GDB_Name)
else:
	print ('gdb already exists')

arcpy.env.workspace =gdb

if arcpy.Exists("st_locations"):
	print ('Delete existing featureclass...')
	arcpy.Delete_management("st_locations")

sr = arcpy.SpatialReference(4326)
print ('create st_locations featureclass...')
arcpy.CreateFeatureclass_management(gdb, "st_locations", "POINT", spatial_reference=sr)
print ('add fields...')
arcpy.management.AddFields('st_locations', [['id', 'LONG'], ['description', 'TEXT']])
print ('Featureclass created: ' + arcpy.Describe('st_locations').name)

