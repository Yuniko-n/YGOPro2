#!/bin/bash
cd ../Assets/SibylSystem/Resources
ls AudioClip/summon/*.wav > AudioClipFile.txt
ls AudioClip/attack/*.wav >> AudioClipFile.txt
ls AudioClip/activate/*.wav >> AudioClipFile.txt
echo "[sound] 记录完成"
