#! /bin/sh
#bin/stage_file input
while read line; do
  bin/create_work --appname physiome1 --command_line "$line" input
  #echo $line
done < creatework.aorticstenosis
