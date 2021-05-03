use std::{collections::VecDeque, fmt, io, u8};

macro_rules! parse_input {
    ($x:expr, $t:ident) => {
        $x.trim().parse::<$t>().unwrap()
    };
}

#[derive(PartialEq, Debug)]
pub enum CellKind {
    Fog,
    Wall,
    Space,
    Source,
    Target,
}

impl CellKind {
    fn from(f: char) -> CellKind {
        match f {
            '?' => CellKind::Fog,
            '#' => CellKind::Wall,
            '.' => CellKind::Space,
            'T' => CellKind::Source,
            'C' => CellKind::Target,
            _ => panic!("unknown cett type {}", f),
        }
    }
}

#[derive(Clone, Copy)]
pub struct Point {
    pub x: i32,
    pub y: i32,
}

impl Point {
    fn new(x: i32, y: i32) -> Point {
        Point { x, y }
    }
}

impl fmt::Display for Point {
    // This trait requires `fmt` with this exact signature.
    fn fmt(&self, f: &mut fmt::Formatter) -> fmt::Result {
        // Write strictly the first element into the supplied output
        // stream: `f`. Returns `fmt::Result` which indicates whether the
        // operation succeeded or failed. Note that `write!` uses syntax which
        // is very similar to `println!`.
        write!(f, "{} {}", self.x, self.y)
    }
}

pub struct Cell {
    pub idx: usize,
    pub pos: Point,
    pub kind: CellKind,
    pub neighbors: [usize; 4],
}

impl Cell {
    pub fn new(idx: usize, pos: Point) -> Self {
        Self {
            idx,
            pos,
            kind: CellKind::Fog,
            neighbors: [usize::MAX, usize::MAX, usize::MAX, usize::MAX],
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
                let cell = Cell::new(y * w + x, Point::new(x as i32, y as i32));
                map.storage.push(cell)
            }
        }

        for y in 0..h {
            for x in 0..w {
                let cell = (y as usize) * map.width + (x as usize);
                map.register_all_neighbors(x as i32, y as i32, cell);
            }
        }

        map
    }

    fn get_mut(&mut self, x: i32, y: i32) -> &mut Cell {
        match self
            .storage
            .get_mut((y as usize) * self.width + (x as usize))
        {
            None => panic!("no item at {} {}", x, y),
            Some(c) => c,
        }
    }

    fn get_safe_mut(&mut self, x: i32, y: i32) -> Option<&mut Cell> {
        self.storage
            .get_mut((y as usize) * self.width + (x as usize))
    }

    pub fn get(&self, x: i32, y: i32) -> &Cell {
        self.storage
            .get((y as usize) * self.width + (x as usize))
            .unwrap()
    }

    fn set_cell_status(&mut self, x: i32, y: i32, status: CellKind) {
        if status == CellKind::Fog {
            return;
        }
        let mut cell = self.get_mut(x, y);
        if cell.kind == status {
            return;
        }
        cell.kind = status;
        if cell.kind == CellKind::Wall {
            let x = cell.pos.x;
            let y = cell.pos.y;
            let neighbor = cell.idx;
            self.remove_all_neighbors(x, y, neighbor);
        }
    }

    fn remove_all_neighbors(&mut self, x: i32, y: i32, neighbor: usize) {
        self.remove_neighbor(x + 1, y, neighbor);
        self.remove_neighbor(x - 1, y, neighbor);
        self.remove_neighbor(x, y + 1, neighbor);
        self.remove_neighbor(x, y - 1, neighbor);
    }
    fn remove_neighbor(&mut self, x: i32, y: i32, neighbor: usize) {
        if let Some(cell) = self.get_safe_mut(x, y) {
            for i in 0..cell.neighbors.len() {
                if cell.neighbors[i] == neighbor {
                    cell.neighbors[i] = usize::MAX;
                    return;
                }
            }
        }
    }

    fn register_all_neighbors(&mut self, x: i32, y: i32, neighbor: usize) {
        self.register_neighbor(x + 1, y, neighbor);
        self.register_neighbor(x - 1, y, neighbor);
        self.register_neighbor(x, y + 1, neighbor);
        self.register_neighbor(x, y - 1, neighbor);
    }
    fn register_neighbor(&mut self, x: i32, y: i32, neighbor: usize) {
        if let Some(cell) = self.get_safe_mut(x, y) {
            for i in 0..cell.neighbors.len() {
                if cell.neighbors[i] == usize::MAX {
                    cell.neighbors[i] = neighbor;
                    return;
                }
            }
        }
    }

    fn point_to_idx(&self, p: Point) -> usize {
        (p.y * self.width as i32 + p.x) as usize
    }

    fn idx_to_point(&self, idx: usize) -> Point {
        if let Some(cell) = self.storage.get(idx) {
            cell.pos
        } else {
            panic!("no cell at {}", idx)
        }
    }

    fn explore(&self, from: usize) -> Option<usize> {
        let mut frontier: VecDeque<usize> = VecDeque::new();
        let mut visited: Vec<usize> = Vec::new();

        visited.resize(self.storage.len(), 0xffff);

        frontier.push_front(from);
        visited[from as usize] = from;

        /* Construct field for tracer */
        while !frontier.is_empty() {
            let p = frontier.pop_front();

            // stop expanding if reached target point
            let cell = self.storage.get(p.unwrap()).unwrap();

            //eprintln!("{}, {:?}", cell.pos, cell.kind);

            if cell.kind == CellKind::Fog {
                return Some(cell.idx);
            }

            for i in 0..cell.neighbors.len() {
                let n = cell.neighbors[i];
                if n != usize::MAX {
                    if visited[n] == 0xffff {
                        visited[n] = p.unwrap();
                        frontier.push_back(n);
                    }
                }
            }
        }
        None
    }

    fn trace(&self, from: usize, to: usize) -> Option<Vec<usize>> {
        let mut frontier: VecDeque<Breadcrump> = VecDeque::new();
        let mut path: Vec<usize> = Vec::new();
        let mut visited: Vec<Option<Breadcrump>> = Vec::new();

        visited.resize(self.storage.len(), None);

        let crump = Breadcrump {
            origin: from,
            hops: 0,
        };
        frontier.push_front(crump);
        visited[from as usize] = Some(crump);

        let mut is_found = false;
        while !frontier.is_empty() {
            let p = frontier.pop_front().unwrap();

            // stop expanding if reached target point
            if p.origin == to {
                is_found = true;
                break;
            }

            let cell = self.storage.get(p.origin).unwrap();
            if cell.kind == CellKind::Fog {
                continue;
            }

            for i in 0..cell.neighbors.len() {
                let neighbor = cell.neighbors[i];
                if neighbor != usize::MAX {
                    if visited[neighbor].is_none() {
                        let from_crump = Breadcrump {
                            origin: p.origin,
                            hops: p.hops,
                        };
                        visited[neighbor] = Some(from_crump);
                        frontier.push_back(Breadcrump {
                            origin: neighbor,
                            hops: p.hops + 1,
                        });
                    }
                }
            }
        }

        if !is_found {
            return None;
        }

        /* Follow the White rabbit */
        let mut p = to;

        path.push(p);

        while p != from {
            let breadcrump = visited[p].unwrap();
            p = breadcrump.origin;
            //eprintln!("pp {} {}", breadcrump.hops, self.idx_to_point(p));
            path.push(p);
        }

        path.reverse();

        return Some(path);
    }
}

#[derive(Clone, Copy)]
struct Breadcrump {
    origin: usize,
    hops: i32,
}

#[derive(PartialEq, Eq, Debug)]
enum GamePhase {
    Explore,
    Trigger,
    Flee,
}

struct Game {
    phase: GamePhase,
    alarm_timer: i32,
    maze: Map,
    start: Point,
    kirk: Point,
    target: Option<Point>,
}

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
fn main() {
    let mut input_line = String::new();
    io::stdin().read_line(&mut input_line).unwrap();
    let inputs = input_line.split(" ").collect::<Vec<_>>();
    let total_rows = parse_input!(inputs[0], i32); // number of rows.
    let total_cols = parse_input!(inputs[1], i32); // number of columns.
    let alarm_timer = parse_input!(inputs[2], i32); // number of rounds between the time the alarm countdown is activated and the time the alarm goes off.

    let mut game = Game {
        phase: GamePhase::Explore,
        alarm_timer: alarm_timer,
        maze: Map::new(total_rows as usize, total_cols as usize),
        start: Point::new(-1000, -1000),
        kirk: Point::new(-1000, -1000),
        target: None,
    };

    // game loop
    loop {
        let mut input_line = String::new();
        io::stdin().read_line(&mut input_line).unwrap();
        let inputs = input_line.split(" ").collect::<Vec<_>>();
        let kr = parse_input!(inputs[0], i32); // row where Kirk is located.
        let kc = parse_input!(inputs[1], i32); // column where Kirk is located.

        if game.start.x == -1000 {
            game.start = Point::new(kc, kr);
        }

        game.kirk = Point::new(kc, kr);

        for row in 0..total_rows as usize {
            let mut input_line = String::new();
            io::stdin().read_line(&mut input_line).unwrap();
            let input_row = input_line.trim().to_string(); // C of the characters in '#.TC?' (i.e. one line of the ASCII maze).

            let mut col = 0;
            for letter in input_row.chars() {
                let status = CellKind::from(letter);

                if status == CellKind::Target {
                    game.target = Some(Point::new(col, row as i32));
                }
                game.maze.set_cell_status(col, row as i32, status);
                col += 1;
            }
        }

        if game.phase == GamePhase::Explore {
            do_explore(&mut game);
        }

        if game.phase == GamePhase::Trigger {
            do_trigger(&mut game);
        }

        if game.phase == GamePhase::Flee {
            let target = game.start;
            got_to(&mut game, target);
        }
        // Write an action using println!("message...");
        // To debug: eprintln!("Debug message...");

        //println!("RIGHT"); // Kirk's next move (UP DOWN LEFT or RIGHT).
    }
}

fn do_trigger(game: &mut Game) -> () {
    if let Some(target) = game.target {
        if game.kirk.x == target.x && game.kirk.y == target.y {
            game.phase = GamePhase::Flee;
            return;
        }

        got_to(game, target);
    }
}

fn do_explore(game: &mut Game) -> () {
    if let Some(target) = game.target {
        let from = game.maze.point_to_idx(game.start);
        let to = game.maze.point_to_idx(target);
        if let Some(back_path) = game.maze.trace(from, to) {
            eprintln!("path {} alarm at {}", back_path.len(), game.alarm_timer);
            if back_path.len() - 1 <= game.alarm_timer as usize {
                game.phase = GamePhase::Trigger;
                return;
            }
        }
    }

    let from = game.maze.point_to_idx(game.kirk);
    let can_explore = game.maze.explore(from);

    if let Some(to_explore) = can_explore {
        //eprintln!("{} {}", to_explore, game.maze.idx_to_point(to_explore));
        got_to(game, game.maze.idx_to_point(to_explore));
    } else {
        panic!("NO explore");
    }
}

fn got_to(game: &mut Game, target: Point) {
    let from = game.maze.point_to_idx(game.kirk);
    let to = game.maze.point_to_idx(target);
    let path = game.maze.trace(from, to).unwrap();
    eprintln!("path len {}", path.len());
    let next_point = game.maze.idx_to_point(path[1]);
    let direction = get_direction(game.kirk, next_point);
    println!("{}", direction);
}

fn get_direction(a: Point, b: Point) -> &'static str {
    if a.x < b.x {
        return "RIGHT";
    }
    if a.x > b.x {
        return "LEFT";
    }

    if a.y < b.y {
        return "DOWN";
    }
    if a.y > b.y {
        return "UP";
    }
    panic!();
}
