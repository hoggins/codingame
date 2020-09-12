use std::ops::Index;
use std::ops::IndexMut;

macro_rules! parse_input {
  ($x:expr, $t:ident) => {
    $x.trim().parse::<$t>().unwrap()
  };
}

// Write an action using println!("message...");
// To debug: eprintln!("Debug message...");
fn main() {
  let mut gs = input::parse_init();

  // game loop
  loop {
    input::parse_tick(&mut gs);

    // first line for movement commands, second line for POD purchase (see the protocol in the statement for details)
    // println!("WAIT");
    // println!("1 73");

    try_seek(&gs);
    println!();

    try_buy(&gs);
    println!();
  }
}

fn try_seek(gs: &GameState) {
  let mut anyCommand = false;
  for idx in 0..gs.map.cells.len() {
    let zone = gs.map.zone(idx as i32);
    if zone.my_pods == 0 {
      continue;
    }
    for i in 0..zone.links.len() {
      let link_idx = zone.links[i];
      let link = gs.map.zone(link_idx);
      if link.owner_id == gs.my_id {
        continue;
      }
      print!("{} {} {} ", zone.my_pods, idx, link_idx);
      anyCommand = true;
    }
  }

  if !anyCommand {
    print!("WAIT");
  }
}

fn try_buy(gs: &GameState) {
  if gs.platinum < 20 {
    print!("WAIT");
    return;
  }
  let mut anyCommand = false;
  let mut platinum = gs.platinum;
  for idx in 0..gs.map.cells.len() {
    let zone = gs.map.zone(idx as i32);
    if zone.owner_id != -1 && zone.owner_id != gs.my_id {
      continue;
    }
    if zone.platinum_source == 0 {
      continue;
    }
    print!("{} {} ", 1, idx);
    platinum -= 20;
    if platinum < 20 {
      return;
    }
    anyCommand = true;
  }
}

#[derive(Clone, Debug)]
pub struct Cell {
  platinum_source: i32,
  links: Vec<i32>,
  owner_id: i32,
  my_pods: i32,
  enemy_pods: Vec<i32>,
}

impl Cell {
  fn new() -> Cell {
    Cell {
      platinum_source: 0,
      links: Vec::new(),
      owner_id: -1,
      my_pods: 0,
      enemy_pods: vec![0; 5],
    }
  }
}

pub struct Map {
  zone_count: i32,
  cells: Vec<Cell>,
}

impl Map {
  fn new(zone_count: i32) -> Map {
    Map {
      zone_count: zone_count,
      cells: vec![Cell::new(); zone_count as usize],
    }
  }

  fn zone(&self, idx: i32) -> &Cell {
    self.cells.index(idx as usize)
  }
  fn zone_mut(&mut self, idx: i32) -> &mut Cell {
    self.cells.index_mut(idx as usize)
  }
}

pub struct GameState {
  player_count: i32,
  my_id: i32,
  map: Map,
  platinum: i32,
}

impl GameState {
  fn new(player_count: i32, my_id: i32, zone_count: i32) -> GameState {
    GameState {
      player_count: player_count,
      my_id: my_id,
      map: Map::new(zone_count),
      platinum: 0,
    }
  }
}

mod input {
  use crate::GameState;
  use std::io;

  pub fn parse_init() -> GameState {
    let mut input_line = String::new();
    io::stdin().read_line(&mut input_line).unwrap();
    let inputs = input_line.split(" ").collect::<Vec<_>>();
    let player_count = parse_input!(inputs[0], i32); // the amount of players (2 to 4)
    let my_id = parse_input!(inputs[1], i32); // my player ID (0, 1, 2 or 3)
    let zone_count = parse_input!(inputs[2], i32); // the amount of zones on the map
    let link_count = parse_input!(inputs[3], i32); // the amount of links between all zones

    let mut gs = GameState::new(player_count, my_id, zone_count);

    for _ in 0..zone_count as usize {
      let mut input_line = String::new();
      io::stdin().read_line(&mut input_line).unwrap();
      let inputs = input_line.split(" ").collect::<Vec<_>>();
      let zone_id = parse_input!(inputs[0], i32); // this zone's ID (between 0 and zoneCount-1)
      let platinum_source = parse_input!(inputs[1], i32); // the amount of Platinum this zone can provide per game turn

      let mut zone = gs.map.zone_mut(zone_id);
      zone.platinum_source = platinum_source;
    }
    for _ in 0..link_count as usize {
      let mut input_line = String::new();
      io::stdin().read_line(&mut input_line).unwrap();
      let inputs = input_line.split(" ").collect::<Vec<_>>();
      let zone_idx_1 = parse_input!(inputs[0], i32);
      let zone_idx_2 = parse_input!(inputs[1], i32);

      let zone = gs.map.zone_mut(zone_idx_1);
      zone.links.push(zone_idx_2);
      let zone = gs.map.zone_mut(zone_idx_2);
      zone.links.push(zone_idx_1);
    }

    gs
  }

  pub fn parse_tick(gs: &mut GameState) {
    let mut input_line = String::new();
    io::stdin().read_line(&mut input_line).unwrap();
    gs.platinum = parse_input!(input_line, i32); // my available Platinum
    for _ in 0..gs.map.zone_count as usize {
      let mut input_line = String::new();
      io::stdin().read_line(&mut input_line).unwrap();
      let inputs = input_line.split(" ").collect::<Vec<_>>();
      let z_id = parse_input!(inputs[0], i32); // this zone's ID
      let owner_id = parse_input!(inputs[1], i32); // the player who owns this zone (-1 otherwise)
      let mut zone = gs.map.zone_mut(z_id);
      zone.owner_id = owner_id;
      for i in 0..4 {
        let pods = parse_input!(inputs[i + 2], i32);
        if i as i32 == gs.my_id {
          zone.my_pods = pods;
        } else {
          zone.enemy_pods[i] = pods;
        }
      }
    }
  }
}
