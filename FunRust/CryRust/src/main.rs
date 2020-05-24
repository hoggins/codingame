use std::convert::TryInto;
use std::io;
use std::collections::HashSet;

macro_rules! parse_input {
    ($x:expr, $t:ident) => ($x.trim().parse::<$t>().unwrap())
}

#[derive(Clone)]
struct Cell {
    ore:i8,
    hole:bool,
}

struct Map {
    grid:Vec<Cell>,
    height:usize,
    width:usize,
    visible_ore:i32,
}

struct Entity {
    id: i32,
    pos: Point,
    kind: EntityKind,
}

enum EntityKind {
    OwnRobot(Robot),
    EnemyRobot(Robot),
    Item(ItemKind),
}

struct Robot {
    item: ItemKind
}

#[derive(PartialEq)]
enum ItemKind {
    None,
    Radar,
    Trap,
    Ore,
}

struct Point {
    x: usize,
    y: usize,
}

struct Context {
    radar_cooldown:i32,
    trap_cooldown:i32,
    entities: Vec<Entity>,
    map:Map,
}

/**
 * Deliver more ore to hq (left side of the map) than your opponent. Use radars to find ore but beware of traps!
 **/
fn main() {
    let mut cx = init();    

    let mut radar_taken = false;

    // game loop
    loop {
        read_tick(&mut cx);

        for entity in &cx.entities {
            match &entity.kind {
                EntityKind::OwnRobot(r) => {
                    match r.item {
                        ItemKind::Radar => {println!("DIG 5 5")},
                        ItemKind::Ore => {println!("MOVE {} {}", 0, entity.pos.y)}
                        _=>{
                            if cx.map.visible_ore == 0 && cx.radar_cooldown == 0 
                                && !radar_taken && ItemKind::None == r.item  {
                                radar_taken = true;
                                println!("REQUEST RADAR")
                            }
                            else if cx.map.visible_ore > 0 {
                                let x = find_ore(&cx.map).unwrap();
                                println!("DIG {} {}", x.0, x.1);
                            }
                            else {
                                println!("WAIT");
                            }
                        }
                    }
                    
                }
                _ => {}
            }
        }
        /*for i in 0..5 as usize {
            // Write an action using println!("message...");
            // To debug: eprintln!("Debug message...");

            println!("WAIT"); // WAIT|MOVE x y|DIG x y|REQUEST item
        }*/
    }
}

impl Map {

    fn get(&self, x: usize, y: usize) -> &Cell { 
        &self.grid[y*self.width + x]
    }

    fn get_mut(&mut self, x: usize, y: usize) -> &mut Cell { 
        &mut self.grid[y*self.width + x]
    }
}

fn find_ore(map: &Map) ->Option<(usize,usize)> {
    for w in 0..map.width {
        for h in 0..map.height {
            let c = map.get(w, h);
            if c.ore > 0 {
                return Some((w,h))
            }
        }
    }
    None
}

fn read_tick(cx: &mut Context) {
    cx.map.visible_ore = 0;
    let mut input_line = String::new();
    io::stdin().read_line(&mut input_line).unwrap();
    let inputs = input_line.split(" ").collect::<Vec<_>>();
    let my_score = parse_input!(inputs[0], i32); // Amount of ore delivered
    let opponent_score = parse_input!(inputs[1], i32);
    for i in 0..cx.map.height as usize {
        let mut input_line = String::new();
        io::stdin().read_line(&mut input_line).unwrap();
        let inputs = input_line.split_whitespace().collect::<Vec<_>>();
        for j in 0..cx.map.width as usize {
            let ore = if inputs[2*j] == "?" {0} else {parse_input!(inputs[2*j], i8)}; // amount of ore or "?" if unknown
            let hole = parse_input!(inputs[2*j+1], i32) == 1; // 1 if cell has a hole
            
            //map.push(Cell { ore: ore, hole: hole });
            let mut cell = &mut cx.map.get_mut(j, i);
            cell.ore = ore;
            cell.hole = hole;

            cx.map.visible_ore += cell.ore as i32;
        }
    }

    let mut touched = HashSet::new();

    let mut input_line = String::new();
    io::stdin().read_line(&mut input_line).unwrap();
    let inputs = input_line.split(" ").collect::<Vec<_>>();
    let entity_count = parse_input!(inputs[0], i32); // number of entities visible to you
    cx.radar_cooldown = parse_input!(inputs[1], i32); // turns left until a new radar can be requested
    cx.trap_cooldown = parse_input!(inputs[2], i32); // turns left until a new trap can be requested
    for i in 0..entity_count as usize {
        let mut input_line = String::new();
        io::stdin().read_line(&mut input_line).unwrap();
        let inputs = input_line.split(" ").collect::<Vec<_>>();
        let entity_id = parse_input!(inputs[0], i32); // unique id of the entity
        let entity_type = parse_input!(inputs[1], i32); // 0 for your robot, 1 for other robot, 2 for radar, 3 for trap
        let x = parse_input!(inputs[2], usize);
        let y = parse_input!(inputs[3], usize); // position of the entity
        let item = parse_input!(inputs[4], i32); // if this entity is a robot, the item it is carrying (-1 for NONE, 2 for RADAR, 3 for TRAP, 4 for ORE)

        if entity_type != 0 {
            continue;
        }

        touched.insert(entity_id);
        let mut e = get_or_new_entity(cx, entity_id);
        e.pos = Point { x: x, y: y};
        match &mut e.kind {
            EntityKind::OwnRobot(x) => {
                x.item = match item {
                    2 => ItemKind::Radar,
                    3 => ItemKind::Trap,
                    4 => ItemKind::Ore,
                    _ => ItemKind::None,
                }
            }
            _ => {}
        } 
    }
    cx.entities.retain(|x| touched.contains(&x.id))
}

fn get_or_new_entity<'a>(cx: &'a mut Context, entity_id: i32) -> &'a mut Entity {
    let idx = cx.entities.iter().position(|e| e.id == entity_id).unwrap_or_else(|| {
        let x = Entity {id:entity_id, pos: Point{x:1000,y:1000}, kind: EntityKind::OwnRobot(Robot { item: ItemKind::None})};
        cx.entities.push(x);
        cx.entities.len() - 1    
    });
    &mut cx.entities[idx]
}

fn init() -> Context {
    let mut input_line = String::new();
    io::stdin().read_line(&mut input_line).unwrap();
    let inputs = input_line.split(" ").collect::<Vec<_>>();
    let width = parse_input!(inputs[0], usize);
    let height = parse_input!(inputs[1], usize); // size of the map

    let mut cx = Context {
        radar_cooldown: 0,
        trap_cooldown: 0,
        entities: vec![],
        map: Map {
            grid: Vec::with_capacity(height*width),
            height: height,
            width: width,
            visible_ore: 0,
        }
    };
    cx.map.grid.resize((cx.map.height * cx.map.width).try_into().unwrap(), Cell{ore:0, hole:false});
    cx
}

