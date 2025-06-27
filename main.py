"""Generate an image that shows the users xp, level and a few other statistics"""

from io import BytesIO
import os

from PIL import Image, ImageDraw, ImageFont
from flask import Flask, request, send_file, make_response
import requests

from werkzeug.middleware.proxy_fix import ProxyFix

app = Flask(__name__)
app.wsgi_app = ProxyFix(app.wsgi_app, x_proto=1, x_host=1)


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
        color: tuple[int, int, int],
    ):
        self.username = username
        self.xp = xp
        self.target_xp = target_xp
        self.level = level
        self.rank = rank
        self.progress = progress
        self.avatar_url = avatar_url
        self.color = color


def draw_rounded_rectangle(draw, xy, radius, fill):
    """Draws a rounded rectangle on the image"""
    x0, y0, x1, y1 = xy
    draw.rectangle([x0 + radius, y0, x1 - radius, y1], fill=fill)
    draw.rectangle([x0, y0 + radius, x1, y1 - radius], fill=fill)
    draw.pieslice([x0, y0, x0 + 2 * radius, y0 + 2 * radius], 180, 270, fill=fill)
    draw.pieslice([x1 - 2 * radius, y0, x1, y0 + 2 * radius], 270, 360, fill=fill)
    draw.pieslice([x0, y1 - 2 * radius, x0 + 2 * radius, y1], 90, 180, fill=fill)
    draw.pieslice([x1 - 2 * radius, y1 - 2 * radius, x1, y1], 0, 90, fill=fill)


def hex_to_rgb(hex_color: str) -> tuple:
    """Converts a hex color string to an RGB tuple."""
    hex_color = hex_color.lstrip("#")
    return tuple(int(hex_color[i : i + 2], 16) for i in (0, 2, 4))


def get_shortened_xp(xp: int) -> str:
    """Converts the xp to a shortened version"""
    if xp < 1000:
        return str(xp)
    elif xp < 1000000:
        return f"{round(xp / 1000, 2)}k"
    else:
        return f"{round(xp / 1000000, 2)}m"


def darken_color(color: tuple[int, int, int], factor: float) -> tuple:
    """Darkens an RGB color by a given factor."""
    return tuple(max(0, int(c * (1 - factor))) for c in color)


def generate_card(user: User):
    """Generates the card image based on the User object passed in"""

    card = Image.new("RGBA", (1326, 400), (0, 0, 0, 0))

    small_font = ImageFont.truetype("SmoochSans-Medium.ttf", 48)
    normal_font = ImageFont.truetype("SmoochSans-Medium.ttf", 62)

    draw = ImageDraw.Draw(card)
    draw_rounded_rectangle(draw, [0, 0, 1326, 400], radius=20, fill="black")

    avatar_data = requests.get(
        user.avatar_url.replace("size=128", "size=256"), timeout=5
    ).content

    avatar = Image.open(BytesIO(avatar_data))
    if avatar.mode != "RGBA":
        avatar = avatar.convert("RGBA")

    mask = Image.new("L", avatar.size, 0)

    draw = ImageDraw.Draw(mask)

    draw.ellipse((0, 0) + avatar.size, fill=255)

    avatar.putalpha(mask)
    card.paste(avatar, (72, 72), avatar)

    draw = ImageDraw.Draw(card)

    draw.text((365, 105), user.username, font=normal_font, fill="white")
    draw.text((365, 190), f"Level {user.level}", font=small_font, fill="white")

    rank_text = f"Rank {user.rank}"
    rank_text_width = draw.textlength(rank_text, font=normal_font)
    start_x = 1254 - rank_text_width

    draw.text(
        (start_x, 105),
        rank_text,
        font=normal_font,
        fill="white",
    )

    xp_text = f"{get_shortened_xp(user.xp)} / {get_shortened_xp(user.target_xp)} XP"
    xp_text_width = draw.textlength(xp_text, font=small_font)
    start_x = 1254 - xp_text_width

    draw.text(
        (start_x, 190),
        xp_text,
        font=small_font,
        fill="white",
    )

    progress_width = 889 * (user.progress / 100)

    draw_rounded_rectangle(
        draw, [365, 252, 1254, 314], radius=5, fill=darken_color(user.color, 0.8)
    )
    draw_rounded_rectangle(
        draw, [365, 252, 365 + progress_width, 314], radius=5, fill=user.color
    )

    return card


@app.route("/cards/<username>", methods=["GET"])
def get_card(username: str):
    """Returns the card image for the user"""
    if not username:
        return "Missing parameters", 400

    try:
        sanitized_username = username.split("?")[0] if "?" in username else username

        sanitized_username = "".join(c for c in username if c.isalnum() or c == "_")
        img_io = open(f"cards/{sanitized_username}.png", "rb")
        response = make_response(
            send_file(
                img_io,
                mimetype="image/png",
                download_name=f"{sanitized_username}-card.png",
            )
        )
        response.headers["Content-Disposition"] = (
            f"attachment; filename={sanitized_username}-card.png"
        )
        response.headers["Content-Type"] = "image/png"

        return response

    except FileNotFoundError:
        return "Image not found", 404


@app.route("/card/new", methods=["POST"])
def new_card():
    """Returns the card image for the user"""

    print(f"Received {request.method} request at /card/new")
    print("Headers:", request.headers)

    data = request.get_json()

    user = None

    try:
        print("Checking for request args")
        username = data.get("username")
        xp = int(data.get("xp"))
        target_xp = int(data.get("target_xp"))
        level = int(data.get("level"))
        rank = int(data.get("rank"))
        progress = int(data.get("progress"))
        avatar_url = data.get("avatar_url")
        color = data.get("color")

        if not all([username, xp, level, rank, progress, avatar_url, color, target_xp]):
            return "Missing parameters", 400

        user = User(
            username,
            xp,
            target_xp,
            level,
            rank,
            progress,
            avatar_url,
            hex_to_rgb(color),
        )
    except TypeError as ex:
        print(f"No request args found. Checking for json: {ex}")
        data = request.json

        if not data:
            return "Missing parameters", 400

        user = User(
            data["username"],
            data["xp"],
            data["target_xp"],
            data["level"],
            data["rank"],
            data["progress"],
            data["avatar_url"],
            hex_to_rgb(data["color"]),
        )

    print(
        f"Generating card for {user.username}: {user.xp} / {user.target_xp} XP, Level {user.level}, Rank {user.rank}, {user.progress}%, {user.avatar_url}, {user.color}"
    )

    card = generate_card(user)

    if not os.path.exists("cards"):
        os.makedirs("cards")

    sanitized_username = "".join(c for c in user.username if c.isalnum() or c == "_")

    card.save(f"cards/{sanitized_username}.png", "PNG")

    return (
        f"/cards/{sanitized_username}?color={data['color'].replace('#', '')}&xp={user.xp}",
        200,
    )


@app.route("/")
def index():
    """Returns the index page"""
    html = """<!DOCTYPE html>
            <html lang="en">

            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>API Index</title>
                <style>
                    body {
                        background-color: #1e1e1e;
                        color: white;
                        font-family: Arial, sans-serif;
                        text-align: center;
                        margin: 0;
                        padding: 20px;
                    }

                    h1 {
                        font-size: 2.5em;
                        margin-bottom: 10px;
                    }

                    p {
                        font-size: 1.2em;
                        opacity: 0.8;
                    }

                    .container {
                        max-width: 600px;
                        margin: auto;
                        padding: 20px;
                        border-radius: 10px;
                        background: rgba(255, 255, 255, 0.1);
                    }

                    .endpoint {
                        font-size: 1.1em;
                        background: rgba(255, 255, 255, 0.2);
                        padding: 10px;
                        border-radius: 5px;
                        margin: 10px 0;
                    }

                    .image-container {
                        margin-top: 20px;
                    }

                    .image-container img {
                        width: 100%;
                        max-width: 300px;
                        border-radius: 10px;
                        background: white;
                        padding: 5px;
                    }
                </style>
            </head>

            <body>
                <div class="container">
                    <h1>API Status</h1>
                    <p>If you see this, it's running ✅</p>

                    <h2>Available Endpoints</h2>
                    <div class="endpoint">🔹 <code>POST /card/new</code> - Create a new card</div>
                    <div class="endpoint">🔹 <code>GET /cards/{username}</code> - Retrieve a user’s card</div>

                    <div class="image-container">
                        <h2>Example Image</h2>
                        <img src="cards/KaenguruuDev.png" alt="Example API Card">
                        <h2>Default Image</h2>
                        <img src="cards/Default" alt="Default API Card">
                    </div>
                </div>
            </body>

            </html>"""
    return html


if __name__ == "__main__":
    app.run(host="127.0.0.1", port=2052, ssl_context=("cert.pem", "key.pem"))
    print(app.url_map)
