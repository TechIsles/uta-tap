#!/bin/bash
rm -rf ./output
mkdir ./output
mkdir ./output/js
mkdir ./output/css
cp -r ./www/* ./output
npm install uglify-js uglifycss -g
uglifyjs ./www/js/utatap.js -o ./output/js/utatap.js
uglifycss ./www/css/utatap.css --output ./output/css/utatap.css
cp -r ./data ./output
rm ./output/data/README.md
ls -N1U ./output/data/vocal > ./output/data/vocal.txt
ls -N1U ./output/data/music > ./output/data/music.txt
