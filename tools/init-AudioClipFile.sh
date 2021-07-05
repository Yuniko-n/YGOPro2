#!/bin/bash
cd ../Assets/SibylSystem/Resources
ls AudioClip/summon/*.wav   >  AudioClip/AudioClipFile.txt
ls AudioClip/attack/*.wav   >> AudioClip/AudioClipFile.txt
ls AudioClip/activate/*.wav >> AudioClip/AudioClipFile.txt
echo "[sound] 记录完成"
