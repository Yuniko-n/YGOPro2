#!/bin/bash
#需要安装 dialog xclip xdotool
#此脚本用于Linux无法输入中文
#可将此脚本设置快捷键方便使用
#游戏内已设置为快捷键：F11

xclip="/usr/bin/xclip"
xdotool="/usr/bin/xdotool"

#安装需要的软件 (暂时只支持：Ubuntu)
install()
{
    if cat /etc/issue | grep -q -E -i "ubuntu"; then
        pkg_install
    elif cat /proc/version | grep -q -E -i "ubuntu"; then
        pkg_install
    else
        #echo -e "\033[31m不支持，因为我没用过，所以不知道怎么安装，23333\033[0m"
        gnome-terminal -t "Tips!" -x bash -c "echo -e '抱歉！暂时只支持：Ubuntu\n请手动安装：dialog xclip xdotool';exec bash;"
    fi
}
#打开新窗口 (因为可能需要输入密码)
pkg_install()
{
    #顺便安装libgdiplus
    gnome-terminal -t "YGOPro2 install" -x bash -c "sudo apt install dialog xclip xdotool libgdiplus -y"
}

#执行
insert()
{
    gdialog --title "YGO Chinese inpu" --inputbox " " 30 80 2>/tmp/ygo_inpu.txt #qt环境的用户请把 gdialog 改为 kdialog
    cat /tmp/ygo_inpu.txt | tr -d '\n' | xclip -i -selection clipboard
    #xdotool key "Shift+Insert"
    xdotool key "Ctrl+v"
}

#检查安装与执行
if [ -f "$xclip" ] && [ -f "$xdotool" ]; then
    insert
else
    install
fi
