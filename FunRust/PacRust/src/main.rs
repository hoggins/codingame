use std::io;
use std::collections::HashSet;

macro_rules! parse_input {
    ($x:expr, $t:ident) => ($x.trim().parse::<$t>().unwrap())
}

#[derive(Clone)]
struct Point {
    x: i8,
    y: i8,
}

impl Point {
    fn new(x: i8, y: i8) -> Point {
        Point{x, y}
    }
}

struct Pac {
    id: i8,
    pos: Point,
    is_mine: bool,
    speed_turns_left: i8,
    ability_cooldown: i8,
}

impl Pac {
    fn new(id: i8, pos: Point, is_mine: bool, speed_turns_left: i8, ability_cooldown: i8) -> Pac {
        Pac{
            id, pos, is_mine, speed_turns_left, ability_cooldown
        }
    }
}

struct Cell {
    pos: Point,
    pellet: i8,
    wall: bool,
}

struct Map {
    grid: Vec<Cell>,
    width: usize,
    height: usize,
}

impl Map {
    fn get_mut(&mut self, x:i8, y:i8) -> &mut Cell {
        self.grid.get_mut((y as usize)*self.width + (x as usize)).unwrap()
    }
}

struct Context {
    map: Map,
    pacs: Vec<Pac>,
}

fn apply_visibility(pacs:&Vec<Pac>, map:&mut Map) {
    for pac in pacs {
        apply_visibility_by_x(map, pac.pos.clone(), 1);
        apply_visibility_by_x(map, pac.pos.clone(), -1);
        apply_visibility_by_y(map, pac.pos.clone(), 1);
        apply_visibility_by_y(map, pac.pos.clone(), -1);
    }
}

fn apply_visibility_by_x(map:&mut Map, pos:Point, dir:i8) {
    let mut x = pos.x;
    for _ in 0..map.width {
        x += dir;
        warp(&mut x, map.width);
        let mut cell = map.get_mut(x, pos.y);
        if cell.wall {
            return;
        }
        set_visible(&mut cell);
    }
}

fn apply_visibility_by_y(map:&mut Map, pos:Point, dir:i8) {
    let mut y = pos.y;
    for _ in 0..map.width {
        y += dir;
        warp(&mut y, map.height);
        let mut cell = map.get_mut(pos.x, y);
        if cell.wall {
            return;
        }
        set_visible(&mut cell);
    }
}

fn set_visible(cell: &mut Cell) {
    cell.pellet = 0;
}

fn warp(v:&mut i8, max:usize) -> i8
{
    match v {
        -1 => { max as i8 }
        _ => { if *v == max as i8 {0} else {v.clone()} }
    }
}

/**
 * Grab the pellets as fast as you can!
 **/
fn main() {
    let mut cx = init();

    // game loop
    loop {
        read_scores(&mut cx);
        read_pacs(&mut cx);
        apply_visibility(&cx.pacs, &mut cx.map);
        read_pellets(&mut cx.map);

        // Write an action using println!("message...");
        // To debug: eprintln!("Debug message...");

        println!("MOVE 0 15 10"); // MOVE <pacId> <x> <y>
    }
}

fn read_scores(cx: &mut Context) {
    let mut input_line = String::new();
    io::stdin().read_line(&mut input_line).unwrap();
    // let inputs = input_line.split(" ").collect::<Vec<_>>();
    // let my_score = parse_input!(inputs[0], i32);
    // let opponent_score = parse_input!(inputs[1], i32);
}

fn read_pellets(map: &mut Map) {
    let mut input_line = String::new();
    io::stdin().read_line(&mut input_line).unwrap();
    let visible_pellet_count = parse_input!(input_line, i32); // all pellets in sight
    for i in 0..visible_pellet_count as usize {
        let mut input_line = String::new();
        io::stdin().read_line(&mut input_line).unwrap();
        let inputs = input_line.split(" ").collect::<Vec<_>>();
        let x = parse_input!(inputs[0], i8);
        let y = parse_input!(inputs[1], i8);
        let value = parse_input!(inputs[2], i8); // amount of points this pellet is worth

        let mut cell = map.get_mut(x, y);
        cell.pellet = value;
    }
}

fn read_pacs(cx: &mut Context) {
    let mut touched = HashSet::new();
    let mut input_line = String::new();
    io::stdin().read_line(&mut input_line).unwrap();
    let visible_pac_count = parse_input!(input_line, i32); // all your pacs and enemy pacs in sight
    for i in 0..visible_pac_count as usize {
        let mut input_line = String::new();
        io::stdin().read_line(&mut input_line).unwrap();
        let inputs = input_line.split(" ").collect::<Vec<_>>();
        let pac_id = parse_input!(inputs[0], i8); // pac number (unique within a team)
        let mine = parse_input!(inputs[1], i32) == 1; // true if this pac is yours
        let x = parse_input!(inputs[2], i8); // position in the grid
        let y = parse_input!(inputs[3], i8); // position in the grid
        let type_id = inputs[4].trim().to_string(); // unused in wood leagues
        let speed_turns_left = parse_input!(inputs[5], i8); // unused in wood leagues
        let ability_cooldown = parse_input!(inputs[6], i8); // unused in wood leagues

        touched.insert(pac_id);

        match cx.pacs.iter().position(|e| e.id == pac_id && e.is_mine == mine) {
            Some(pacIdx) => {
                let mut pac = cx.pacs.get_mut(pacIdx).unwrap();
                pac.pos = Point::new(x, y);
                pac.speed_turns_left = speed_turns_left;
                pac.ability_cooldown = ability_cooldown;
            }
            None => {
                let pac = Pac {
                    id:pac_id,
                    pos:Point{x:x, y:y},
                    is_mine: mine,
                    speed_turns_left: speed_turns_left,
                    ability_cooldown: ability_cooldown
                };
                cx.pacs.push(pac);
            }
        }
    }
    cx.pacs.retain(|x| touched.contains(&x.id))
}


fn init() -> Context {
    let mut input_line = String::new();
    io::stdin().read_line(&mut input_line).unwrap();
    let inputs = input_line.split(" ").collect::<Vec<_>>();
    let width = parse_input!(inputs[0], usize); // size of the grid
    let height = parse_input!(inputs[1], usize); // top left corner is (x=0, y=0)

    let map = Map {grid:Vec::new(),width:width, height:height};
    let mut cx = Context {map:map, pacs:Vec::new()};

    for i in 0..height as usize {
        let mut input_line = String::new();
        io::stdin().read_line(&mut input_line).unwrap();
        let row = input_line.trim_end().to_string(); // one line of the grid: space " " is floor, pound "#" is wall
        let mut x = 0;
        for c in row.chars() {
            let mut cell = Cell{pos:Point{x:x, y:i as i8}, pellet: 0, wall:false};
            let payload = match c {
                ' ' => {(1, false)}
                _ => {(0, true)}
            };
            cell.pellet = payload.0;
            cell.wall = payload.1;
            x += 1;

            cx.map.grid.push(cell)
        }
    }
    cx
}
