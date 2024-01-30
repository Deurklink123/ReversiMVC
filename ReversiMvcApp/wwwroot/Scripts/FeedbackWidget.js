class FeedbackWidget {
    constructor(elementId) {
        this._elementId = elementId;
    }

    get elementId() { //getter, set keyword voor setter methode
        return this._elementId;
    }

    show(message, type) {
        //code
        var x = document.getElementById(this._elementId);
        $(x).text(message);
        if (type === 'succes') {
            $(x).css('background-color', 'darkgreen');
        } else {
            $(x).css('background-color', 'red');
        }
        x.style.display = "block";
        this.log({ message: message, type: type })
    }


    hide() {
        var x = document.getElementById(this._elementId);
        x.style.display = "none";
    }

    log(message) {
        let arr = JSON.parse(localStorage.getItem("feedback_widget")) ?? [];
        if (arr.length >= 10) arr.splice(0, 1);
        arr.push(message);
        localStorage.setItem("feedback_widget", JSON.stringify(arr));
    }

    removelog() {
        localStorage.removeItem('feedback_widget')
    }

    history() {
        let arr = JSON.parse(localStorage.getItem('feedback_widget'));
        let str;
        for (let i = 0; arr.length > i; i++) {
            str += arr[i].type + "-" + arr[i].message + "\n";
        }
        this.show(str, 'succes');
    }
}

let f = new FeedbackWidget('feedback-success');


$(function () {
    console.log("ready!");
});

$("#ok-btn").on("click", function () {
    alert("The button was clicked.");
});

$("#x-btn").on("click", function () {
    var x = document.getElementById("popup");
    x.classList.replace("fade-in", "fade-out")
});

$("#akkoord-btn").on("click", function () {
    var x = document.getElementById("popup");
    x.classList.replace("fade-in", "fade-out")
});

$("#weigeren-btn").on("click", function () {
    var x = document.getElementById("popup");
    x.classList.replace("fade-in", "fade-out")
});