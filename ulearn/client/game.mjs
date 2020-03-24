const MAX_LOAD_SHIP = 368;

let portsCoordinates = {};
let homePort = {};

export function startGame(levelMap, gameState) {
    homePort = gameState.ports.filter(port => port.isHome)[0];
    portsCoordinates = gameState.ports.filter(port => !port.isHome);
}


export function getNextCommand(gameState) {
    let command = 'WAIT';
    if (canLoadProduct(gameState)) {
        const product = getProductForLoad(gameState);
        if (product)
            command = `LOAD ${product.name} ${product.amount}`
    } else if (needSale(gameState)) {
        const product = getProductForSale(gameState);
        if (product)
            command = `SELL ${product.name} ${product.amount}`
    } else {
        command = goto(gameState);
    }
    return command;
}

//9760
function canLoadProduct(gameState) {
    return gameState.ship.goods.length === 0 && isHomePort(gameState.ship);
}

function isInTradePort(ship) {
    const portsArray = portsCoordinates.filter(port => weAreIn(ship, port));
    return !!portsArray;
}

function getPriceByPortId(prices, portId) {
    return prices.filter(price => price.portId === portId)[0];
}


// gamestate.ship
function isHomePort(ship) {
    return weAreIn(ship, homePort);
}

function weAreIn(obj1, obj2) {
    return obj1.x === obj2.x && obj1.y === obj2.y;
}


function getProductForLoad({goodsInPort, prices, }) {

    const products = goodsInPort.map(good => {
        return {
            'name': good.name,
            'max_price': Math.max(...prices.map(port_price => port_price[good.name])),
            'amount': Math.floor(MAX_LOAD_SHIP / good.volume),
        }
    });
    console.log(products)
    const priceWithAmount = (product) => product && product.max_price * product.amount;

    const optimalProduct = products.reduce((p, v) => {
        return ( priceWithAmount(p) > priceWithAmount(v) ? p : v );
    }, null);
    return optimalProduct;
}


function needSale(gameState) {
    return gameState.ship.goods.length > 0 && isInTradePort(gameState.ship) &&
        weAreIn(findOptimalPort(gameState), gameState.ship)
}


function getProductForSale({ship}) {
    const priceWithAmount = (product) => product && [product.name]*product.amount;
    const product = ship.goods.reduce((obj1, obj2) => {
        return ( priceWithAmount(obj1) > priceWithAmount(obj2) ? obj1 : obj2 );
    }, null);
    return product;
}

function profitOnSale(ship, port, price) {
    let profit = 0;
    if (!port.isHome && price) {
        profit = ship.goods.map((val, i, arr) => price[val.name] * val.amount).reduce((a, b) => a + b, profit);
    }
    return profit;
}


function findOptimalPort({ship, ports, prices}) {
    return ports.reduce((max_port, port) => {
        const profitFromCurrentPort = profitOnSale(ship, port, getPriceByPortId(prices, port.portId));
        const profitFromMaxPort = profitOnSale(ship, max_port, getPriceByPortId(prices, max_port.portId));
        if (profitFromCurrentPort > profitFromMaxPort) {
            return port;
        } else {
            return max_port;
        }
    }, ports[0]);
}


function goto(gameState) {
    const ship = gameState.ship;
    const optimalPort = findOptimalPort(gameState);

    if (ship.y > optimalPort.y) {
        return 'N';
    }
    if (ship.y < optimalPort.y) {
        return 'S';
    }
    if (ship.x > optimalPort.x) {
        return 'W';
    }
    if (ship.x < optimalPort.x) {
        return 'E';
    }
    return 'WAIT'
}
