# Restoring legacy planets with ore mining

## A word of warning

Those planets haven't been used for a long time and may exhibit problems like missing assets
or other bad behaviors. Use at your own risk.

Be careful not to have overlapping planets.


## How to import a legacy planet into the game

### Setting correct position and construct id.

First, edit the json file with a text or json editor. Two things need to be changed:

- constructIdHint: this is the construct id the planet will have. It must be between 1 and 100 and not be already used.
- position: x,y,z absolute universe coordinates of the planet. Do not overlap anything.

Do not set any rotation or things will break bad.

### Importing through Backoffice.

Simplest way is to use the backoffice construct import feature: Go to the constructs view, hit the import button.

In the form that shows up select your json file, and tick:

- user construct
- replace if exists


Then hit import. The backoffice should then show the page for your newly imported construct. Check that construct id is correct.

Now restart the server and the planet should be visible.

### Alternative: Importing through backend command


Once done, run the following commands:

    # This will shell to withing the sandbox service mounting current directory as 'input' in the container
    # LINUX VERSION
    docker-compose run -v $PWD:/input --entrypoint bash sandbox
    # WINDOWS VERSION
    docker-compose run -v "%cd%:/input" --entrypoint bash sandbox
    # Run the planet importer
    cd python
    ./server.py -c /config/dual.yaml fixture import_planet /input/legacy-planets-with-ore/ore-alioth.json

Replace "ore-alioth.json" in the last command with your planet file name.


## Final words

A full stack restart is required after importing or deleting a planet.

If you want your planet to appear on the system map you need to add an entry under SolarSystemConfig in backoffice itembank.
