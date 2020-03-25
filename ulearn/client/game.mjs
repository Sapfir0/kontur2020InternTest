const MAX_LOAD_SHIP = 368;

let portsCoordinates = {};
let homePort = {};
let ship;

class Ship {
    x;
    y;
    items;

    constructor(gameState) {
        this.refreshShipState(gameState)
    }

    refreshShipState(gameState) {
        this.x = gameState.x;
        this.y = gameState.y;
        this.items = gameState.goods;
        console.log(gameState)
    }

    moveToSouth() {
        return 'S'
    }

    moveToNorth() {
        return 'N'
    }

    moveToEast() {
        return 'E'
    }

    moveToWest() {
        return 'W'
    }

    wait() {
        return 'W'
    }


}

class Port {
    id;
    coordinates;
    constructor(id, coordinates) {
        this.id = id;
        this.coordinates = coordinates;
    }
}

class HomePort extends Port {

}

class TradingPort extends Port {

}

function canLoadProduct(gameState) {
    return gameState.ship.goods.length === 0 && isHomePort(gameState.ship);
}

export function startGame(levelMap, gameState) {

    homePort = gameState.ports.filter(port => port.isHome)[0];
    //homePort = new HomePort(homePortArray.portId, (homePortArray.x, homePortArray.y));

    portsCoordinates = gameState.ports.filter(port => !port.isHome);
    //portsCoordinates = new TradingPort(portsCoordinatesArray.portId, (portsCoordinatesArray.x, portsCoordinatesArray.y))
    ship = new Ship(gameState.ship)
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



function isInTradePort(ship) {
    const portsArray = portsCoordinates.filter(port => weAreIn(ship, port));
    return !!portsArray;
}

function isHomePort(ship) {
    return weAreIn(ship, homePort);
}

function weAreIn(obj1, obj2) {
    return obj1.x === obj2.x && obj1.y === obj2.y;
}



function getPriceByPortId(prices, portId) {
    return prices.filter(price => price.portId === portId)[0];
}

function getProductForLoad({goodsInPort, prices, }) {

    const products = goodsInPort.map(good => {
        return {
            'name': good.name,
            'max_price': Math.max(...prices.map(port_price => port_price[good.name])),
            'amount': Math.floor(MAX_LOAD_SHIP / good.volume),
        }
    });

    const priceWithAmount = (product) => product && product.max_price * product.amount;

    const optimalProduct = products.reduce((p, v) => {
        return ( priceWithAmount(p) > priceWithAmount(v) ? p : v );
    }, null);
    return optimalProduct;
}


function needSale(gameState) {
    return ship.items.length > 0 && isInTradePort(gameState.ship) &&
        weAreIn(findOptimalPort(gameState), gameState.ship)
}


function getProductForSale({ship, prices, ports}) {
    const port = isInTradePort({ship, ports});
    const priceOnCurrentPort = getPriceByPortId(prices, port.portId);
    const priceWithAmount = (product) => product && (priceOnCurrentPort[product.name]*product.amount);
    return ship.goods.reduce((obj1, obj2) => {
        return (priceWithAmount(obj1) > priceWithAmount(obj2) ? obj1 : obj2);
    }, null);
    return product;
}

function profitOnSale(ship, port, price) {
    let profit = 0;
    if (!port.isHome && price) {
        profit = ship.goods.map((val, i, arr) => (price[val.name]*val.amount) / distance(ship, port)).reduce((a, b) => a+b, 0);
    }
    return profit;
}

function distance(obj1, obj2) {
    return Math.abs(obj1.x-obj2.x)+Math.abs(obj1.y-obj2.y);
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
    const optimalPort = findOptimalPort(gameState);
    ship.refreshShipState(gameState.ship)

    let command;
    if (ship.y > optimalPort.y) {
        command = ship.moveToNorth()
    }
    if (ship.y < optimalPort.y) {
        command = ship.moveToSouth()
    }
    if (ship.x > optimalPort.x) {
        command = ship.moveToWest()
    }
    if (ship.x < optimalPort.x) {
        command = ship.moveToEast()
    }
    if (command === undefined) {
        command = ship.wait()
    }
    return command;
}
