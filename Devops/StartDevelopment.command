#!/bin/bash
here="`dirname \"$0\"`"
echo "cd-ing to $here"
cd "$here" || exit 1
docker-compose -p royal-cards-server -f dev-services-docker-compose up -d
read -p "Press enter to continue"