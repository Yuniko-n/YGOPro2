#!/bin/sh
DESTINATION_DIR=${XDG_DATA_HOME:-~/.local/share}/applications
IN_FILE=YGOPro2_linux_Data/StreamingAssets/YGOPro2_linux.desktop.in
OUT_NAME=YGOPro2_linux.desktop

cd $(dirname $0) &&
sed "s|{INSTALL_PATH}|$PWD|" $IN_FILE > $DESTINATION_DIR/$OUT_NAME &&
chmod +x $DESTINATION_DIR/$OUT_NAME
