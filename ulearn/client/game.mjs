const MAX_LOAD_SHIP = 368;

let portsCoordinates = [];
let homePort = {};
let ship;
let map = [[]];
let lenToPorts = {};
let productVolume = {};


class Ship {
    x = 0;
    y = 0;
    items;

    constructor(gameState) {
        this.refreshShipState(gameState)
    }

    getLocation() {
        const loc = {x: this.x, y: this.y}
        return loc;
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

    getFreeSpaceInShip() {
        return ship.items.reduce((acc, cur) => acc - productVolume[cur.name] * cur.amount, MAX_LOAD_SHIP);
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

    static productProfit(priceInPort, product, len) {
        return priceInPort[product.name] * product.amount / len;
    }

    static amountInShip(freeSpaceShip, product) {
        return  Math.min(Math.floor(freeSpaceShip / product.volume), product.amount);

    }
}


class MapObject {
    x;
    y;
    isHomePort;
    isTradePort;

    constructor(x, y, isHomePort = false, isTradePort = false) {
        this.x = x;
        this.y = y;
        this.isHomePort = isHomePort;
        this.isTradePort = isTradePort;
    }
}


class Node {
    childrens = []
    mapObject;
    parent;

    constructor(mapObject, parent = null) {
        this.mapObject = mapObject;
        this.parent = parent;
    }
}

function matrixArray(rows, columns) {
    var arr = [];
    for (var i = 0; i < rows; i++) {
        arr[i] = [];
        for (var j = 0; j < columns; j++) {
            arr[i][j] = 0;//вместо i+j+1 пишем любой наполнитель. В простейшем случае - null
        }
    }
    return arr;
}

function createMapObject(symbol, x, y) {
    let mapObject;
    switch (symbol) {
        case "O": {
            mapObject = new MapObject(x, y, false, true)
            break;
        }
        case "H": {
            mapObject = new MapObject(x, y, true)
            break;
        }
        case "~": {
            mapObject = new MapObject(x, y)
            break;
        }
    }
    return mapObject;
}


function parseMap(levelMap) {
    const matrix = levelMap.split('\n');
    for (let x = 0; x < matrix.length; x++) {
        matrix[x] = matrix[x].split("")
    }
    const width = matrix.length;
    const height = matrix[0].length
    let matrixAdjasment = matrixArray(width, height);
    //console.log(matrixAdjasment)
    for (let x = 1; x < matrix.length - 1; x++) {
        for (let y = 1; y < matrix[x].length - 1; y++) {
            const currentCell = matrix[x][y];
            const leftCell = new MapObject(x - 1, y)
            const rightCell = new MapObject(x + 1, y);
            const downCell = new MapObject(x, y - 1);
            const upCell = new MapObject(x, y + 1);
            const neighbours = [leftCell, rightCell, downCell, upCell];
            if (currentCell !== "#") {
                const mapObject = createMapObject(currentCell, x, y)
                let node = new Node(mapObject)
                for (let i = 0; i < neighbours.length; i++) {
                    if (matrix[neighbours[i].x][neighbours[i].y] !== "#") {
                        const innerMapObject = createMapObject(matrix[neighbours[i].x][neighbours[i].y], neighbours[i].x, neighbours[i].y)
                        const childrens = new Node(innerMapObject)
                        childrens.parent = node;
                        node.childrens.push(childrens)
                    }
                }
                matrixAdjasment[x][y] = node;
            }
        }
    }
    return matrixAdjasment;
}

export function startGame(levelMap, gameState) {
    //console.log(levelMap)
    //parseMap(levelMap);
    portsCoordinates = [];
    homePort = {};
    map = [[]];
    lenToPorts = {};
    productVolume = {};
    ship = new Ship(gameState.ship);
    homePort = {}
    portsCoordinates = []

    for (let i = 0; i < gameState.ports.length; i++) { // дополним наш массив ценами
        const currentPortId = gameState.ports[i].portId;
        gameState.ports[i].prices = gameState.prices.filter(price => price.portId === currentPortId)[0]
    }

    const homePortArray = gameState.ports.filter(port => port.isHome)[0];
    const portsCoordinatesArray = gameState.ports.filter(port => !port.isHome);

    homePort = new HomePort(homePortArray.portId, homePortArray.x, homePortArray.y);
    portsCoordinatesArray.forEach(port =>
        portsCoordinates.push(new TradingPort(port.portId, port.x, port.y, port.prices)))

    gameState.goodsInPort.forEach(good => {
        productVolume[good.name] = good.volume;
    });
}


export function getNextCommand(gameState) {
    let command = 'WAIT';
    ship.refreshShipState(gameState.ship);

    if (ship.canLoadProduct(gameState)) {
        const product = getProductForLoad(gameState.goodsInPort);
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

function getProductForLoad(goodsInPort) {
    const freeSpaceShip = ship.getFreeSpaceInShip();

    const products = portsCoordinates.map((port, index) => {
        const price = port.prices;
        if (!price) return null;
        let optimalProduct = null;
        let max = 0;
        for (const product of goodsInPort) {
            if (price.hasOwnProperty(product.name)) {
                const amountInShip = Maths.amountInShip(freeSpaceShip, product);
                const profit = price[product.name] * amountInShip;
                if (max < profit) {
                    optimalProduct = {
                        name: product.name,
                        amount: amountInShip
                    };
                    max = profit;
                }
            }
        }
        return {
            product: optimalProduct,
            priceInPort: price,
            port, index
        }
    });
    products.forEach(products => {
        if (products && products.product && !lenToPorts.hasOwnProperty(products.port.portId)) {
            lenToPorts[products.port.portId] = Maths.distance(products.port, homePort);
        }
    });

    const profitObj = reduceIf(products, profitToPort)
    return profitObj && profitObj.product;
}


function profitToPort(obj) {
    return obj && obj.product && Maths.productProfit(obj.priceInPort, obj.product, lenToPorts[obj.port.portId]);
}

function reduceIf(array, callback) {
    const product = array.reduce((obj1, obj2) => {
        if (callback(obj1) > callback(obj2)) {
            return obj1;
        }
        return obj2;
    }, null);
    return product;
}


function getProductForSale() {
    const priceWithAmount = (product) => product && [product.name] * product.amount;
    const product = reduceIf(ship.items, priceWithAmount);

    return product;
}


function profitOnSale(port, price) {
    if (port.isHome) return 0;

    const profit = ship.items.map((val, i, arr) =>
        (price[val.name] * val.amount) / Maths.distance(ship, port))

    const maxProfit = profit.reduce((a, b) => a + b, 0);

    return maxProfit;
}


function findOptimalPort({_, ports, prices}) {
    // portsCoordinates.push(homePort);
    // console.log(portsCoordinates)

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


function goto(gameState) {
    const optimalPort = findOptimalPort(gameState);
    if (optimalPort === undefined) return 'WAIT';

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


class QElement {
    constructor(element, priority) {
        this.element = element;
        this.priority = priority;
    }
}

class PriorityQueue {
    constructor() {
        this.items = [];
    }

    enqueue(element, priority) {
        // creating object from queue element
        var qElement = new QElement(element, priority);
        var contain = false;
        // iterating through the entire item array to add element at the correct location of the Queue
        for (var i = 0; i < this.items.length; i++) {
            if (this.items[i].priority > qElement.priority) {
                // Once the correct location is found it is enqueued
                this.items.splice(i, 0, qElement);
                contain = true;
                break;
            }
        }
        // if the element have the highest priority
        // it is added at the end of the queue
        if (!contain) {
            this.items.push(qElement);
        }
    }

    dequeue() {
        // return the dequeued element and remove it.
        // if the queue is empty returns Underflow
        if (this.isEmpty())
            return "Underflow";
        return this.items.shift();
    }

    front() {
        // returns the highest priority element in the Priority queue without removing it.
        if (this.isEmpty())
            return "No elements in Queue";
        return this.items[0];
    }

    rear() {
        // returns the lowest priorty element of the queue
        if (this.isEmpty())
            return "No elements in Queue";
        return this.items[this.items.length - 1];
    }

    isEmpty() {
        // return true if the queue is empty.
        return this.items.length === 0;
    }

    printPQueue() {
        var str = "";
        for (var i = 0; i < this.items.length; i++)
            str += this.items[i].element + " ";
        return str;
    }
}