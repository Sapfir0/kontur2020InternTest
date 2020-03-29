
let tradePorts = [];
let homePort = {};
let ship;
let map = [];
let distanceToPorts = {};
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
        const directions = [
            {x: -1, y:  0},
            {x:  1, y:  0},
            {x:  0, y: -1},
            {x:  0, y:  1},
        ];
        for(const pirate of pirates) {
            for (const direction of this.directions) {
                const x = pirate.x + direction.x;
                const y = pirate.y+direction.y;
                this.lastPiratesLocatation[y][x] = true;
            }
        }
        //console.log(this.rememberMapObjects)
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

        //console.log(matrixAdjasment)
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
        if (this.lastPiratesLocatation[y][x]) return 0;
        return this.symbolMap[y][x];
    }

    Set(y,x,value) {
        //console.log(this.symbolMap[y][x])
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
            arr[i][j] = 0;//вместо i+j+1 пишем любой наполнитель. В простейшем случае - null
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
    distanceToPorts = {};
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

function maneuvereToPort(source, destination) {
    let elementsInQueue = 0;
    const maxElementsInQueue = 300;
    const queue = new PriorityQueue();

    queue.enqueue({...source, way: []}, 0);
    const visited = createMatrix(map.Height, map.Width);

    while (!queue.isEmpty()) {
        const node = queue.dequeue();

        if (node.element.x === destination.x && node.element.y === destination.y ) {
            return node.element.way;
        }

        elementsInQueue++;
        visited[node.element.y][node.element.x] = true;

        for (const direction of map.directions) {
            const {x, y} = {x:node.element.x + direction.x, y: node.element.y + direction.y}
            if (isReachable({x,y}) && !visited[y][x] ) {
                const generatedWay = [...node.element.way, {x, y}];
                const priority = generatedWay.length + + Maths.manhattanDistance({x, y} , destination)
                const newNode = {x,y, way: generatedWay}
                queue.enqueue(newNode, priority);
            }
        }

        if (elementsInQueue > maxElementsInQueue) {
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
        // creating object from queue element
        const qElement = new QElement(element, priority);
        this.items.push(qElement);
        this.items.sort((a, b) => b.priority - a.priority);
    }

    dequeue() {
        // return the dequeued element and remove it.
        // if the queue is empty returns Underflow
        if (this.isEmpty())
            return null;
        return this.items.pop();
    }


    isEmpty() {
        // return true if the queue is empty.
        return this.items.length === 0;
    }

}


function generateProducts(goodsInPort, freeSpaceShip) {
    const products = tradePorts.map((port, index) => {
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
        return {
            product: optimalProduct,
            priceInPort: price,
            port,
            index
        }
    });
    return products;
}


function getProductForLoad(goodsInPort) {
    const freeSpaceShip = ship.getFreeSpaceInShip();

    const products = generateProducts(goodsInPort, freeSpaceShip);

    for (const product of products) {
        if (product && product.product && !distanceToPorts.hasOwnProperty(product.port.portId)) {
            const way = maneuvereToPort(product.port, homePort);
            let distanceToPort = Infinity;
            if (way !== null) distanceToPort = way.length;
            distanceToPorts[product.port.id] = distanceToPort;
        }
    }

    const maxCostForProduct = maxElement(products, profitToPort);
    const product = maxCostForProduct && maxCostForProduct.product;
    ship.load(product.name, product.amount)
}


function profitToPort(obj) {
    return obj && obj.product && Maths.productProfit(obj.priceInPort, obj.product, distanceToPorts[obj.port.id]);
}

function maxElement(array, comparator, reduceDefaultValue=null) {
    return array.reduce((obj1, obj2) => {
        if (comparator(obj1) > comparator(obj2)) {
            return obj1;
        }
        return obj2;
    }, reduceDefaultValue);
}


function getProductForSale() {
    const priceWithAmount = (product) => product && [product.name] * product.amount;
    const product =  maxElement(ship.items, priceWithAmount);
    ship.sell(product.name, product.amount)
}


function profitOnSale(port) {
    if (port instanceof HomePort || !port.prices) return 0;

    const profit = ship.items.map(function(val, i, arr) {
        return (port.prices[val.name] * val.amount) / Maths.distance(ship, port)
    })

    return profit.reduce((a, b) => a + b, 0);
}


function findOptimalPort() {
    const localPorts = tradePorts;
    localPorts.push(homePort)
    //return maxElement(portes, profitOnSale, homePort)
    return localPorts.reduce((max_port, port) => {
        if (profitOnSale(max_port) < profitOnSale(port)) {
            return port;
        } else {
            return max_port;
        }
    }, homePort);
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

