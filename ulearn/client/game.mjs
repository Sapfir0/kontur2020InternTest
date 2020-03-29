
let tradePorts = [];
let homePort = {};
let ship;
let map = [];
let distanceToPort = {};
let productDesc = {};


class Ship {
    SHIP_HOLD_SIZE = 368;

    command = "";
    x = 0;
    y = 0;
    items;

    constructor(gameState) {
        this.refreshShipState(gameState)
    }

    get Сomand() {
        return this.command;
    }

    set Сomand(command) {
        this.command = command;
    }

    get location() {
        const loc = {x: this.x, y: this.y}
        return loc;
    }

    refreshShipState(gameState) {
        this.x = gameState.x;
        this.y = gameState.y;
        this.items = gameState.goods;
    }

    isInTradePort() {
        const portsArray = tradePorts.filter(port => this.weAreIn(port));
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

    canLoadProduct() {
        return this.getFreeSpaceInShip() > 35 && !ship.notHaveItems() && ship.isHomePort();
    }

    moveToSouth() {
        this.command =  'S'
    }

    moveToNorth() {
        this.command =  'N'
    }

    moveToEast() {
        this.command =  'E'
    }

    moveToWest() {
        this.command = 'W'
    }

    wait() {
        this.command =  'WAIT'
    }

    load(product, amount) {
        this.command = `LOAD ${product} ${amount}`

    }

    sell(product, amount) {
        this.command = `SELL ${product} ${amount}`
    }

    needSale() {
        return ship.isInTradePort() && ship.weAreIn(findOptimalPort())
    }

    getFreeSpaceInShip() {
        return this.items.reduce((allFreeSpace, cur) => allFreeSpace - productDesc[cur.name] * cur.amount, this.SHIP_HOLD_SIZE);
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
        return this.manhattanDistance(obj1, obj2);
    }

    static productProfit(priceInPort, product, len) {
        return priceInPort[product.name] * product.amount / len;
    }

    static amountInShip(freeSpaceShip, product) {
        return  Math.min(Math.floor(freeSpaceShip / product.volume), product.amount);
    }

    static manhattanDistance(obj1, obj2) {
        return Math.abs(obj1.x-obj2.x)+Math.abs(obj1.y-obj2.y);
    }
}


class Map {
    symbolMap;
    lastPiratesLocatation ;
    directions = [
        {x: -1, y:  0},
        {x:  1, y:  0},
        {x:  0, y: -1},
        {x:  0, y:  1},
    ];


    refreshPirates(pirates) {
        this.lastPiratesLocatation = createMatrix(this.Height, this.Width)
        for(const pirate of pirates) {
            for (const direction of this.directions) {
                const x = pirate.x + direction.x;
                const y = pirate.y+direction.y;
                this.lastPiratesLocatation[y][x] = true;
            }
        }
    }

    constructor(levelMap) {
        const matrix = levelMap.split('\n');
        for (let x = 0; x < matrix.length; x++) {
            matrix[x] = matrix[x].split("")
        }

        const width = matrix.length;
        const height = matrix[0].length
        let matrixAdjasment = createMatrix(width, height);
        this.lastPiratesLocatation = createMatrix(width, height)

        for (let x = 1; x < matrix.length - 1; x++) {
            for (let y = 1; y < matrix[x].length - 1; y++) {
                const currentCell = matrix[x][y];
                const neighbours = [
                    createMapObject(currentCell, x - 1, y),
                    createMapObject(currentCell,x + 1, y),
                    createMapObject(currentCell, x, y - 1),
                    createMapObject(currentCell, x, y + 1)
                ];
                if (currentCell !== "#") {
                    let childrens = [];
                    for (const neighbour of neighbours) {
                        if (matrix[neighbour.x][neighbour.y] !== "#") {
                            const innerMapObject = createMapObject(matrix[neighbour.x][neighbour.y], neighbour.x, neighbour.y)
                            childrens.push(innerMapObject)
                        }
                    }
                    const mapObject = createMapObject(currentCell, x, y, childrens)
                    matrixAdjasment[x][y] = mapObject;
                }
            }
        }
        this.symbolMap = matrixAdjasment;
    }


    get Height() {
        return this.symbolMap.length;
    }

    get Width() {
        return this.symbolMap[0].length;
    }

    Get(y, x) {
        if (this.lastPiratesLocatation[y][x]) {
            return 0;
        }
        return this.symbolMap[y][x];
    }

    Set(y,x,value) {
        this.symbolMap[y][x] = value;
    }

}

class MapObject {
    x;
    y;
    isHomePort;
    isTradePort;
    symbol;
    neighbours = [];

    constructor(symbol, x, y, neighbours=[], isHomePort = false, isTradePort = false) {
        this.symbol = symbol;
        this.x = x;
        this.y = y;
        this.neighbours = neighbours;
        this.isHomePort = isHomePort;
        this.isTradePort = isTradePort;
    }
}


function createMatrix(rows, columns) {
    const arr = [];
    for (let i = 0; i < rows; i++) {
        arr[i] = [];
        for (let j = 0; j < columns; j++) {
            arr[i][j] = 0;
        }
    }
    return arr;
}

function createMapObject(symbol, x, y, neighbours = []) {
    let mapObject;

    switch (symbol) {
        case "O": {
            mapObject = new MapObject(symbol, x, y, neighbours, false, true)
            break;
        }
        case "H": {
            mapObject = new MapObject(symbol, x, y,  neighbours,true)
            break;
        }
        case "~": {
            mapObject = new MapObject(symbol, x, y, neighbours)
            break;
        }
    }
    return mapObject;
}




export function startGame(levelMap, gameState) {
    tradePorts = [];
    homePort = {};
    distanceToPort = {};
    productDesc = {};
    ship = new Ship(gameState.ship);
    homePort = {}
    map = new Map(levelMap);


    for (let gameStatePort of gameState.ports) {
        const currentPortId = gameStatePort.portId;
        gameStatePort.prices = gameState.prices.filter(price => price.portId === currentPortId)[0]
    }

    const homePortArray = gameState.ports.filter(port => port.isHome)[0];
    const portsCoordinatesArray = gameState.ports.filter(port => !port.isHome);

    homePort = new HomePort(homePortArray.portId, homePortArray.x, homePortArray.y);
    portsCoordinatesArray.forEach(port =>
        tradePorts.push(new TradingPort(port.portId, port.x, port.y, port.prices)))

    for (const product of gameState.goodsInPort) {
        productDesc[product.name] = product.volume
    }
}


export function getNextCommand(gameState) {
    ship.refreshShipState(gameState.ship);
    map.refreshPirates(gameState.pirates);

    if (ship.canLoadProduct()) getProductForLoad(gameState.goodsInPort);
    else if (ship.needSale()) getProductForSale();
    else goto();

    return ship.command;
}


function isReachable(cell) {
    return cell.x >= 0 &&
        cell.x < map.Width &&
        cell.y >= 0 &&
        cell.y < map.Height &&
        map.Get(cell.y, cell.x) != 0;
}

function maneuvereToPort(objSource, objDestination) {
    const queue = new PriorityQueue();
    queue.enqueue({...objSource, way: []}, 0);
    const visited = createMatrix(map.Height, map.Width);
    let counter = 0;
    while (!queue.isEmpty()) {
        const node = queue.dequeue();

        if (node.element.x === objDestination.x && node.element.y === objDestination.y ) {
            return node.element.way;
        }

        counter++;
        visited[node.element.y][node.element.x] = true;

        for (const direction of map.directions) {
            const new_node = {
                x: node.element.x + direction.x,
                y: node.element.y + direction.y
            };
            if (!visited[new_node.y][new_node.x] && isReachable(new_node)) {
                const {x, y} = new_node;
                new_node.way = [...node.element.way, {x, y}];
                queue.enqueue(new_node, new_node.way.length + Maths.manhattanDistance(new_node, objDestination));
            }
        }

        if (counter > 300) {
            break;
        }
    }
    return null;
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
        const qElement = new QElement(element, priority);
        this.items.push(qElement);
        this.items.sort((a, b) => b.priority - a.priority);
    }

    dequeue() {
        return this.items.pop();
    }


    isEmpty() {
        return this.items.length === 0;
    }

}


function generateProducts(goodsInPort, freeSpaceShip) {
    let products = []
    for (const [i, port] of tradePorts.entries()) {
        if (!port.prices) return null;
        const price = port.prices;
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
        products.push({
            product: optimalProduct,
            port: port,
            index: i
        })

    }

    return products;
}


function getProductForLoad(goodsInPort) {
    const freeSpaceShip = ship.getFreeSpaceInShip();
    const products = generateProducts(goodsInPort, freeSpaceShip);

    for (const product of products) {
        if (product && product.product && !distanceToPort.hasOwnProperty(product.port.portId)) {
            distanceToPort[product.port.portId] = Maths.distance(product.port, homePort);
        }
    }

    const maxCostForProduct = maxElement(products, profitToPort)
    const product = maxCostForProduct && maxCostForProduct.product;
    ship.load(product.name, product.amount)
}


function profitToPort(obj) {
    return obj && obj.product && Maths.productProfit(obj.port.prices, obj.product, distanceToPort[obj.port.portId]);
}

function maxElement(array, comparator, reduceDefaultValue=null) {
    const product = array.reduce((obj1, obj2) => {
        if (comparator(obj1) > comparator(obj2)) {
            return obj1;
        }
        return obj2;
    }, reduceDefaultValue);
    return product;
}


function getProductForSale() {
    const priceWithAmount = (product) => product && [product.name] * product.amount;
    const product =  maxElement(ship.items, priceWithAmount);
    ship.sell(product.name, product.amount)
}


function profitOnSale(port) {
    if (port instanceof HomePort || !port.prices) {
        return 0;
    }

    const profit = ship.items.map(function(val, i, arr) {
        return (port.prices[val.name] * val.amount) / Maths.distance(ship, port)
    })

    return sum(profit)
}

function sum(array) {
    return array.reduce((sum, elem) => sum + elem, 0);

}

function findOptimalPort() {
    let localPorts = []
    for (const port of tradePorts) {
        localPorts.push(port)
    }
    localPorts.push(homePort)

    //return maxElement(portes, profitOnSale, homePort)
    const port = localPorts.reduce((max_port, port) => {
        if (profitOnSale(max_port) < profitOnSale(port)) {
            return port;
        } else {
            return max_port;
        }
    }, homePort);
    return port
}


function goto() {
    const optimalPort = findOptimalPort();
    if (optimalPort === undefined) {
        ship.wait()
    }
    const way = maneuvereToPort(ship, optimalPort);
    let destination = way[0];
    if (destination === undefined) {
        destination = optimalPort
    }

    if (ship.y > destination.y) {
        ship.moveToNorth()
    }
    if (ship.y < destination.y) {
        ship.moveToSouth()
    }
    if (ship.x > destination.x) {
        ship.moveToWest()
    }
    if (ship.x < destination.x) {
        ship.moveToEast()
    }
}


