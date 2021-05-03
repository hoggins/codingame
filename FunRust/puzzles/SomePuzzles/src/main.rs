use std::{io, u8};

macro_rules! parse_input {
    ($x:expr, $t:ident) => {
        $x.trim().parse::<$t>().unwrap()
    };
}

enum CellKind {
    Fog,
    Wall,
    Space,
    Target,
    Myself,
}

impl CellKind {
    fn from(f: char) -> CellKind {
        match f {
            '?' => CellKind::Fog,
            '#' => CellKind::Wall,
            '.' => CellKind::Space,
            'T' => CellKind::Target,
            'C' => CellKind::Myself,
            _ => panic!("unknown cett type {}", f),
        }
    }
}

#[derive(Clone)]
pub struct Point {
    pub x: i8,
    pub y: i8,
}

impl Point {
    fn new(x: i8, y: i8) -> Point {
        Point { x, y }
    }
}

pub struct Cell {
    pub idx: usize,
    pub pos: Point,
    pub kind: CellKind,
}

impl Cell {
    pub fn new(idx: usize, pos: Point) -> Self {
        Self {
            idx,
            pos,
            kind: CellKind::Fog,
        }
    }
}

struct Map {
    pub height: usize,
    pub width: usize,
    pub storage: Vec<Cell>,
}

impl Map {
    fn new(h: usize, w: usize) -> Self {
        let mut map = Self {
            height: h,
            width: w,
            storage: Vec::<Cell>::new(),
        };

        for y in 0..h {
            for x in 0..w {
                let mut cell = Cell::new(y * w + x, Point::new(x as i8, y as i8));
                map.storage.push(cell)
            }
        }

        map
    }

    fn get_mut(&mut self, x: i8, y: i8) -> &mut Cell {
        match self
            .storage
            .get_mut((y as usize) * self.width + (x as usize))
        {
            None => panic!("no item at {} {}", x, y),
            Some(c) => c,
        }
    }

    pub fn get(&self, x: i8, y: i8) -> &Cell {
        self.storage
            .get((y as usize) * self.width + (x as usize))
            .unwrap()
    }
}

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
fn main() {
    let mut input_line = String::new();
    io::stdin().read_line(&mut input_line).unwrap();
    let inputs = input_line.split(" ").collect::<Vec<_>>();
    let r = parse_input!(inputs[0], i32); // number of rows.
    let c = parse_input!(inputs[1], i32); // number of columns.
    let a = parse_input!(inputs[2], i32); // number of rounds between the time the alarm countdown is activated and the time the alarm goes off.

    // game loop
    loop {
        let mut input_line = String::new();
        io::stdin().read_line(&mut input_line).unwrap();
        let inputs = input_line.split(" ").collect::<Vec<_>>();
        let kr = parse_input!(inputs[0], i32); // row where Kirk is located.
        let kc = parse_input!(inputs[1], i32); // column where Kirk is located.
        let mut maze = Map::new();
        for i in 0..r as usize {
            let mut input_line = String::new();
            io::stdin().read_line(&mut input_line).unwrap();
            let row = input_line.trim().to_string(); // C of the characters in '#.TC?' (i.e. one line of the ASCII maze).
            let line: Vec<CellKind> = row.chars().map(|f| CellKind::from(f)).collect();
            maze.add(line);
        }

        // Write an action using println!("message...");
        // To debug: eprintln!("Debug message...");

        println!("RIGHT"); // Kirk's next move (UP DOWN LEFT or RIGHT).
    }
}
