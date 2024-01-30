let SPA = (function () {
    var bord = [
        [0, 0, 0, 0, 0, 0, 0, 0],
        [0, 0, 0, 0, 0, 0, 0, 0],
        [0, 0, 0, 0, 0, 0, 0, 0],
        [0, 0, 0, 1, 2, 0, 0, 0],
        [0, 0, 0, 2, 1, 0, 0, 0],
        [0, 0, 0, 0, 0, 0, 0, 0],
        [0, 0, 0, 0, 0, 0, 0, 0],
        [0, 0, 0, 0, 0, 0, 0, 0]
    ];
    var speler;
    var beurt;
    var spelerToken;
    var spelToken = document.getElementById("spelToken").innerText;
//variabel met welke of dit device speler 1 of 2 is.
    //deze info halen uit een div of var of iets van de spelpagina.cshtml.
    //persoon die als eerste joint is peler 1 dus dan is var voor speler 2 nog leeg.
    const createGamePlay = function () {

        //dom elemet Spa selecteren
        //let gameplayElement = document.getElementById('SPA');

        //template ophalen + parse
        //let geparsteTemplate = SPA.templating.parseTemplate('speelbord.gameplay');

        //geparste gameplay toevoegen aan DOM
        //gameplayElement.innerHTML = geparsteTemplate;

        const collection = document.getElementsByClassName("kolom");
        let y = 0;
        let x = 0;
        let index = 0;

        for (const element of collection) {
            if(x>7){
                x=0
                y++;
            }
            element.setAttribute("id", y + "," + x);
            element.setAttribute("onclick", "SPA.doMove(this)");
            //console.log(element);
            x++;
        }

    }

    const getSpeler = function () {
        var speler2T = document.getElementById("Speler2Token");
        if (speler2T.innerText === "") {
            console.log("je bent speler1");
            speler = 1;
            spelerToken = document.getElementById("Speler1Token").innerText;
        }
        else {
            console.log("je bent speler2");
            speler = 2;
            spelerToken = speler2T.innerText;
        }
    }
    
    const doMove = async function (element) {
        await updateAanDeBeurt();
        console.log(beurt);
        if (beurt === speler) {
            $.post('https://localhost:5001/api/Spel/doeZet',  // url
                {
                    SpelToken: spelToken,
                    SpelerToken: spelerToken,
                    cords: element.id,
                    pas: false
                }, // data to be submit
                function (data, status, xhr) {   // success callback function
                    //console.log('status: ' + status + ', data: ' + data);
                    if (!data) {
                        alert('Mag niet');
                    }

                },
                'json'); // response data format
        }
        else {
            alert("Wacht op je beurt");
        }

        //console.log("Clicked: " + element.id);
    }

    const pas = async function () {
        await updateAanDeBeurt();
        if (beurt === speler) {
        $.post('https://localhost:5001/api/Spel/doeZet',  // url
            {
                SpelToken: spelToken,
                SpelerToken: spelerToken,
                cords: "",
                pas: true
            }, // data to be submit
            function (data, status, xhr) {   // success callback function
                if (!data) {
                    alert('Mag niet');
                }
            },
            'json'); // response data format
        }
        else {
            alert("Wacht op je beurt");
        }
    }

    const updateAanDeBeurt = async function () {
        await $.get('https://localhost:5001/api/Spel/Beurt?SpelToken=' + spelToken, (result) => {
            beurt = JSON.parse(result);
        });
    }

    const getBord = async function () {
        await $.get('https://localhost:5001/api/Spel/Spel?SpelToken=' + spelToken, (result) => {
            const spel = JSON.parse(result);
            if (spel.Bord.toString() !== bord.toString()) {
                bord = spel.Bord;
                updateBord();
            }
        });
    }

    const updateBord = async function () {
        for (var y = 0; y <= 7; y++) {
            for (var x = 0; x <= 7; x++) {
                if (bord[y][x] != 0) {
                    document.getElementById(y + "," + x).innerHTML = "<div class='fiche-" + bord[y][x] + " fiche-animation'></div>";;
                }
            }
        }
    }

    const init = function () {
        createGamePlay();
        getSpeler();
        updateAanDeBeurt();
        updateBord();
        setInterval(getBord, 2000);
    }

    return {
        init,
        doMove,
        pas
    }

})();

/*SPA.templating = (function () {

    let parseTemplate = function (templatePath, data) {
        let template = getTemplateFunc(templatePath);
        if (!template) {
            throw new Error('Template not found: ' + templatePath);
        }
        return template(data);
    }

    let getTemplateFunc = function (templatePath) {
        let currentPOJO = window.spa_templates.templates;
        let tpl;
        if (templatePath.includes('.')) {
            let templatePathArray = templatePath.split('.');
            for (let i = 0; i < templatePathArray.length; i++) {
                let propertyPath = templatePathArray[i];
                currentPOJO = currentPOJO[propertyPath];
            }
            tpl = currentPOJO;
        }
        return tpl;
    }

    return {
        parseTemplate: parseTemplate,
        getTemplate: getTemplateFunc
    }

})();*/


document.addEventListener('DOMContentLoaded', () => {
    SPA.init();
})



