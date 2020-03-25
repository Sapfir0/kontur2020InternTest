const MAX_LOAD_SHIP = 368;

let portsCoordinates = {};
let homePort = {};
let ship;

class Ship {
    x = 0;
    y = 0;
    items;

    constructor(gameState) {
        this.refreshShipState(gameState)
    }

    refreshShipState(gameState) {
        this.x = gameState.x;
        this.y = gameState.y;
        this.items = gameState.goods;
    }

    isInTradePort() {
        const portsArray = portsCoordinates.filter(port => this.weAreIn(port));
        return !!portsArray;
    }

    isHomePort() {
        return this.weAreIn(homePort);
    }

    weAreIn(something) {
        return this.x === something.x && this.y === something.y;
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

    notHaveItems() {
        return ship.items.length > 0
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
    return gameState.ship.goods.length === 0 && ship.isHomePort(gameState.ship);
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
    ship.refreshShipState(gameState.ship)
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

    // let optimalProduct = 0;
    // for(let i=0; i<products.length - 1; i++) {
    //     console.log(priceWithAmount(products[i]) > priceWithAmount(products[i+1]))
    //     if (priceWithAmount(products[i]) > priceWithAmount(products[i+1]) && priceWithAmount(products[i]) > optimalProduct) {
    //         optimalProduct = products[i];
    //     }
    // }
    // console.log(optimalProduct1)
    // console.log(optimalProduct)
    //console.log(products)

    return optimalProduct;
}


function needSale(gameState) {
    return ship.notHaveItems() && ship.isInTradePort() && ship.weAreIn(findOptimalPort(gameState))
}

function maxWithAmount(priceOnCurrentPort, obj1, obj2) {
    const priceWithAmount = (product) => product && console.log("foo") && (priceOnCurrentPort[product.name]*product.amount);
    const price1 = priceWithAmount(obj1);
    const price2 = priceWithAmount(obj2);
    if (price1 > price2) return obj1; else return obj2;
}


function getProductForSale({prices, ports}) {
    const port = ship.isInTradePort({ship, ports});
    const priceOnCurrentPort = getPriceByPortId(prices, port.portId);
    return ship.items.reduce((obj1, obj2) => {
        return maxWithAmount(priceOnCurrentPort, obj1, obj2)
    }, null);
}


function distance(obj1, obj2) {
    return Math.abs(obj1.x - obj2.x) + Math.abs(obj1.y - obj2.y);
}


function profitOnSale(ship, port, price) {
    let profit = 0;
    if (!port.isHome && price) {
        profit = ship.goods.map((val, i, arr) => (price[val.name]*val.amount) / distance(ship, port)).reduce((a, b) => a+b, 0);
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
    const optimalPort = findOptimalPort(gameState);

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
