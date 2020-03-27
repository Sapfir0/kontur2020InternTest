import { renderMap, renderTextOverMap, renderInterface, removeTextOverMap } from './render.mjs';
import levels from './levels.mjs';
import startGameLoop from "./gameCore.mjs";

drawDefaultMap();
addListeners();

function drawDefaultMap () {
    const map = getDefaultMap();
    renderMap(map);
    renderTextOverMap('Выбери уровень');
}

function getDefaultMap () {
    return `
~~~~~~~~~~~~~~~~####
~~~~~~~~~~~~~~~~~O##
~~~~~~O~~~~~~~~~~~##
~~~~~###~~~~#~~~~~~~
~~~~~##~~~~~##~~~~~~
##~~~~~~~~~~~#~~~~~~
##~~~~~~~~~~~~~~~~~#
#~~~~~~~~~####~~~###
~~~~~~~~~###H~~~~~##
~~~##~~~~~####~~~~~~
~~####~~~~~~~~~~~~~~
~~#O#~~~~~~~~~~~~~~#
~~~~~~~~~~~~~~~~~~O#
~~~~~~~~~~~~~~~~~###
~~~~~~~~~~##~~~~~~##
#~~~~~~~~####O~~~~~#
##~~~~~~~~####~~~~~~
#~~~~~~~~~~O##~~~~~~
~~~~~~~~~~~~#~~~~~~~
~~~~~~~~~~~~~~~~~~~~
`.trim();
}

addSelectorListener();
function addSelectorListener(){
    const selector = document.querySelector('.level-select');

    selector.addEventListener('change', () => {
        const levelNumber = selector.value;
        const level = levels[levelNumber];
        if (!level) {
            return;
        }
        const map = level.map;
        const state = level.state;

        removeTextOverMap();
        renderMap(map);
        renderInterface(state);
        renderTextOverMap('Уровень ' + levelNumber);
    });
}

function addListeners () {
    const form = document.querySelector('.level-select-form');

    form.addEventListener('submit', evt => {
        evt.preventDefault();
        const level = form.level.value;
        runNewGame(level);
    });
}

function runNewGame (levelNumber) {
    removeTextOverMap();
    const level = levels[levelNumber];
    if (!level) {
        return;
    }
    const map = level.map;
    const state = level.state;

    renderMap(map);
    renderInterface(state);
    startGameLoop(map, state, level.piratesPoints);
}
