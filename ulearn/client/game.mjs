const MAX_LOAD_SHIP = 368;

let portsCoordinates = [];
let homePort = {};
let ship;
let map = [];

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

    notHaveItems() {
        return ship.items.length > 0
    }

    canLoadProduct(gameState) {
        return !ship.notHaveItems() && ship.isHomePort(gameState.ship);
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

    needSale(gameState) {
        return ship.notHaveItems() && ship.isInTradePort() && ship.weAreIn(findOptimalPort(gameState))
    }

}

class Port {
    id;
    x;
    y;
    constructor(id, x, y) {
        this.id = id;
        this.x = x;
        this.y = y;
    }
}

class HomePort extends Port {

}

class TradingPort extends Port {
    prices;
    constructor(id, x, y, prices) {
        super(id, x, y);
        this.prices = prices;
    }
}

class Maths {
    static distance(obj1, obj2) {
        return Math.abs(obj1.x - obj2.x) + Math.abs(obj1.y - obj2.y);
    }

    static maxWithAmount(priceOnCurrentPort, obj1, obj2) {
        const priceWithAmount = (product) => product && console.log("foo") && (priceOnCurrentPort[product.name]*product.amount);
        const price1 = priceWithAmount(obj1);
        const price2 = priceWithAmount(obj2);
        if (price1 > price2) return obj1; else return obj2;
    }

    static priceWithAmount(product) {
        return product && product.max_price * product.amount;

    }
}


class MapObject {
    reachable;
    x;
    y;
    isHomePort;
    isTradePort;
    constructor(x,y,reachable,isHomePort=false, isTradePort=false) {
        this.x = x;
        this.y = y;
        this.reachable = reachable;
        this.isHomePort = isHomePort;
        this.isTradePort = isTradePort;
    }
}



class Node {
    nodeLeft = null;
    nodeRight = null;
    data;
    parent;
    constructor(data, parent=null) {
        this.data = data;
        this.parent = parent;
    }
}




function parseMap(levelMap) {
    let x=0;
    let y=0;
    const rows = levelMap.split('\n');
    rows.forEach(row => {
        const cell = row.split("")
        cell.forEach(symbol => {
            if (symbol === "~") {
                map.push(new MapObject(x,y,true))
            } else if(symbol === "#") {
                map.push(new MapObject(x,y,false))
            } else {
                if (symbol === "O") {
                    map.push(new MapObject(x,y,true, false, true))
                }
                else if(symbol === "H") {
                    map.push(new MapObject(x,y,true, true))
                }
            }
            x++;
        });
        y++;
        x=0;
    });

}

export function startGame(levelMap, gameState) {
    console.log(levelMap)
    parseMap(levelMap);

    ship = new Ship(gameState.ship);

    for (let i=0; i<gameState.ports.length; i++) { // дополним наш массив
        const currentPortId = gameState.ports[i].portId;
        gameState.ports[i].prices = getPriceByPortId(gameState.prices, currentPortId);
    }

    const homePortArray = gameState.ports.filter(port => port.isHome)[0];
    const portsCoordinatesArray = gameState.ports.filter(port => !port.isHome);

    homePort = new HomePort(homePortArray.portId, homePortArray.x, homePortArray.y);
    portsCoordinatesArray.forEach(port =>
        portsCoordinates.push(new TradingPort(port.portId, port.x, port.y, port.prices)))

}


export function getNextCommand(gameState) {
    let command = 'WAIT';
    ship.refreshShipState(gameState.ship);

    if (ship.canLoadProduct(gameState)) {
        const product = getProductForLoad(gameState);
        if (product)
            command = `LOAD ${product.name} ${product.amount}`
    } else if (ship.needSale(gameState)) {
        const product = getProductForSale();
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


    const optimalProduct = products.reduce((p, v) => {
        return ( Maths.priceWithAmount(p) > Maths.priceWithAmount(v) ? p : v );
    }, null);
    // let optimalProduct = {};
    // for(let i=0; i<products.length -1; i++) {
    //     console.log(optimalProduct)
    //     console.log(products[i])
    //     if (optimalProduct < products[i] || optimalProduct < products[i+1]) {
    //         if (priceWithAmount(products[i]) > priceWithAmount(products[i+1])) {
    //             optimalProduct = products[i];
    //         }
    //         else {
    //             optimalProduct = products[i+1];
    //         }
    //     }
    // }

    console.log(optimalProduct)
    return optimalProduct;
}


function getProductForSale() {
    const priceWithAmount = (product) => product && [product.name]*product.amount;
    const product = ship.items.reduce((obj1, obj2) => {
        if (priceWithAmount(obj1) > priceWithAmount(obj2)) {
            return obj1;
        }
        return obj2;
    }, null);
    return product;
}


function profitOnSale(port, price) {
    let profit = 0;
    if (!port.isHome && price) {
        profit = ship.items.map((val, i, arr) =>
            (price[val.name]*val.amount) / Maths.distance(ship, port)).reduce((a, b) => a+b, 0);
    }
    return profit;
}



function findOptimalPort({_, ports, prices}) {
    return ports.reduce((max_port, port) => {
        const profitFromCurrentPort = profitOnSale(port, getPriceByPortId(prices, port.portId));
        const profitFromMaxPort = profitOnSale(max_port, getPriceByPortId(prices, max_port.portId));
        if (profitFromCurrentPort > profitFromMaxPort) {
            return port;
        } else {
            return max_port;
        }
    }, ports[0]);
}


function isUnlockedWay(x, y) {
    return map.filter(mapObject => mapObject.x === x && mapObject.y === y)[0].reachable;
}

function goto(gameState) {
    const optimalPort = findOptimalPort(gameState);

    const currentLocation = (ship.x, ship.y);
    if (currentLocation) {

    }
    debugger
    let command;
    if (ship.y > optimalPort.y) {
        if (isUnlockedWay(ship.x, ship.y-1)) {
            command = ship.moveToNorth()
        }
    }
    if (ship.y < optimalPort.y) {
        if (isUnlockedWay(ship.x, ship.y+1)) {
            command = ship.moveToSouth()
        }
    }
    if (ship.x > optimalPort.x) {
        if (isUnlockedWay(ship.x-1, ship.y)) {
            command = ship.moveToWest()
        }
    }
    if (ship.x < optimalPort.x) {
        if (isUnlockedWay(ship.x+1, ship.y)) {
            command = ship.moveToEast()
        }
    }
    if (command === undefined) {
        command = ship.wait()
    }
    return command;
}



