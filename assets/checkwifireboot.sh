#!/bin/sh
PING_IP="192.168.178.1"

echo "Pinging $PING_IP"
ping -c4 $PING_IP > /home/pi/Apps/Harvester/wificheckresult.txt

if [ $? != 0 ]
then
  echo "No network connection - rebooting."
  sudo /sbin/shutdown -r now
fi
