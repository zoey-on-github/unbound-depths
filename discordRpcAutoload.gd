extends Node

func _ready():
	DiscordRPC.app_id =  1349996644959256619
	if (DiscordRPC.get_is_discord_working()):
		print("Established Connection w/ Discord ...!")
	else:
		print("Connection w/ Discord Failed ...")
	DiscordRPC.state = "julie's playtesting"
	DiscordRPC.start_timestamp = int(Time.get_unix_time_from_system())
	DiscordRPC.large_image = "julieatwork"
	DiscordRPC.small_image = "julieatwork"
	DiscordRPC.refresh()
	queue_free()
