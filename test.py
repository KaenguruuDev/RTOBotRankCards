"""asdf"""

import requests


class User:
    """A class to represent a user"""

    def __init__(
        self,
        username: str,
        xp: int,
        target_xp: int,
        level: int,
        rank: int,
        progress: int,
        avatar_url: str,
        color: str,
    ):
        self.username = username
        self.xp = xp
        self.target_xp = target_xp
        self.level = level
        self.rank = rank
        self.progress = progress
        self.avatar_url = avatar_url
        self.color = color


def try_get_image():
    """asf"""
    user = User(
        "Kaenguruu",
        2934,
        5384,
        10,
        5,
        int((2934 / 5384) * 100),
        "https://cdn.discordapp.com/avatars/720176646044385280/3fd01bc4eec3db8ce5b13314a195a9f9.png?size=128",
        "#FF0000",
    )

    # try create a new card
    result = requests.post(
        "https://localhost:2002/card/new",
        params=user.__dict__,
        stream=True,
        timeout=5,
        verify=False,
    )

    # now query it
    result = requests.get(
        f"https://localhost:2002/{result.content.decode()}",
        stream=True,
        timeout=5,
        verify=False,
    )

    with open("KaenguruuTest.png", "wb") as f:
        f.write(result.content)


try_get_image()
