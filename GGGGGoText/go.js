// wait until the page is loaded to draw the board
$(document).ready(function () {
    drawBoard();
});

// this function just draws the background
function drawBoard() {
    // load the image first
    $("canvas").drawImage({
        source: "board.jpg",
        x: 10, y: 10,
        width: 360,
        height: 360,
        fromCenter: false,
        load: drawBoardLines //after image loads, this callback draws the lines on top
    });
}

// this function draws the lines on top of the board image
function drawBoardLines() {
    // draw the border
    $("canvas").drawRect({
        strokeStyle: "#000",
        x: 0, y: 0,
        width: 380,
        height: 380,
        fromCenter: false,
        cornerRadius: 5
    });

    // draw horizontal lines
    for (i = 0; i < 19; i++) {
        var px1 = 10;
        var px2 = 370;
        var py1 = (i * 20) + 10;

        $("canvas").drawLine({
            strokeStyle: "#000",
            strokeWidth: 2,
            x1: px1, y1: py1,
            x2: px2, y2: py1
        });
    }

    // draw vertical lines
    for (i = 0; i < 19; i++) {
        var px1 = (i * 20) + 10;
        var py1 = 10;
        var py2 = 370;

        $("canvas").drawLine({
            strokeStyle: "#000",
            strokeWidth: 1,
            x1: px1, y1: py1,
            x2: px1, y2: py2
        });
    }

    // draw starpoints
    for (i = 0; i < 3; i++) {
        var px = (i * 120) + 70;

        for (j = 0; j < 3; j++) {

            var py = (j * 120) + 70;

            $("canvas").drawArc({
                strokeStyle: "#000",
                strokeWidth: 5,
                x: px, y: py,
                radius: 2
            });
        }
    }
}