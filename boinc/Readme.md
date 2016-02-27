```apps/*''' versions of physiome project worker files
```boinc_server_test/*``` modified files of /var/lib/boinc_server_test/physiome

# prepare boinc project name "physiome"
get http://cz.archive.ubuntu.com/ubuntu/pool/universe/p/python-support/python-support_1.0.14ubuntu2_all.deb
sudo dpkg -i python-support_1.0.14ubuntu2_all.deb 
sudo apt-get install boinc-server-maker

gedit .boinc_test.conf 
./make_project
python setup_project.py 

# copy ```boinc_server_test/physiome/*``` to /var/lib/boinc_server_test/physiome

# go to project directory
cd /var/lib/boinc-server-test/physiome

# start status stop boinc server
bin/start
bin/status
bin/stop

#create work - custom scripts
./creatework.sh
./creatework2.sh


# manually run sample validator
nohup bin/sample_trivial_validator --app physiome1 &
tail -f nohup.out

# manually run sample assimilator for project
nohup bin/sample_assimilator --app physiome1 &
tail -f nohup.out

# manually run sample validator & assimilator - custom script
./validateassimilate.sh
