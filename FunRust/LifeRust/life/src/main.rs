use std::io;
use std::ops::Index;

macro_rules! parse_input {
  ($x:expr, $t:ident) => {
    $x.trim().parse::<$t>().unwrap()
  };
}

/**
 * Bring data on patient samples from the diagnosis machine to the laboratory with enough molecules to produce medicine!
 **/
fn main() {
  let mut input_line = String::new();
  io::stdin().read_line(&mut input_line).unwrap();
  let project_count = parse_input!(input_line, i32);
  for i in 0..project_count as usize {
    let mut input_line = String::new();
    io::stdin().read_line(&mut input_line).unwrap();
    let inputs = input_line.split(" ").collect::<Vec<_>>();
    let a = parse_input!(inputs[0], i32);
    let b = parse_input!(inputs[1], i32);
    let c = parse_input!(inputs[2], i32);
    let d = parse_input!(inputs[3], i32);
    let e = parse_input!(inputs[4], i32);
  }

  // game loop
  loop {
    let robots = input::parse_robots();

    let mut input_line = String::new();
    io::stdin().read_line(&mut input_line).unwrap();
    let inputs = input_line.split(" ").collect::<Vec<_>>();
    let available_a = parse_input!(inputs[0], i32);
    let available_b = parse_input!(inputs[1], i32);
    let available_c = parse_input!(inputs[2], i32);
    let available_d = parse_input!(inputs[3], i32);
    let available_e = parse_input!(inputs[4], i32);

    let samples = input::parse_samples();

    try_think(robots, samples);

    // Write an action using println!("message...");
    // To debug: eprintln!("Debug message...");

    //println!("GOTO DIAGNOSIS");
  }
}

fn try_think(robots: Vec<Robot>, samples: Vec<Sample>) {
  let myself = robots.index(0);
  let cur_sample = samples.iter().find(|&x| x.carried_by == 0);

  if cur_sample.is_none() {
    let sample_to_take = samples
      .iter()
      .filter(|&x| x.carried_by == -1)
      .min_by_key(|&x| x.cost_a + x.cost_b + x.cost_c + x.cost_d + x.cost_e);
    if sample_to_take.is_some() {
      if myself.target != "DIAGNOSIS" {
        println!("GOTO DIAGNOSIS");
      } else {
        let sample = sample_to_take.unwrap().sample_id;
        println!("CONNECT {}", sample)
      }
    } else if myself.target != "SAMPLES" {
      println!("GOTO SAMPLES");
    } else {
      println!("CONNECT 2");
    }
    return;
  }

  let cur_sample = cur_sample.unwrap();
  if cur_sample.is_undiagnosed() {
    if myself.target != "DIAGNOSIS" {
      println!("GOTO DIAGNOSIS");
    } else {
      let sample = cur_sample.sample_id;
      println!("CONNECT {}", sample)
    }
    return;
  }

  let c = cur_sample;
  eprintln!(
    "have sample a{} b{} c{} d{} e{}",
    c.cost_a, c.cost_b, c.cost_c, c.cost_d, c.cost_e
  );

  let storage_cap =
    myself.storage_a + myself.storage_b + myself.storage_c + myself.storage_d + myself.storage_e;
  let sample_cap = cur_sample.cost_a
    + cur_sample.cost_b
    + cur_sample.cost_c
    + cur_sample.cost_d
    + cur_sample.cost_e;
  if storage_cap != sample_cap {
    if myself.target != "MOLECULES" {
      println!("GOTO MOLECULES");
    } else {
      if try_mol_connect(myself.storage_a, cur_sample.cost_a, "A".to_string())
        || try_mol_connect(myself.storage_b, cur_sample.cost_b, "B".to_string())
        || try_mol_connect(myself.storage_c, cur_sample.cost_c, "C".to_string())
        || try_mol_connect(myself.storage_d, cur_sample.cost_d, "D".to_string())
        || try_mol_connect(myself.storage_e, cur_sample.cost_e, "E".to_string())
      {}
    }
    return;
  }

  if myself.target != "LABORATORY" {
    println!("GOTO LABORATORY");
  } else {
    println!("CONNECT {}", cur_sample.sample_id);
  }
}

fn try_mol_connect(storage: i32, cost: i32, name: String) -> bool {
  if storage >= cost {
    return false;
  }
  println!("CONNECT {}", name);
  return true;
}

mod input {
  use crate::Robot;
  use crate::Sample;
  use std::io;

  pub fn parse_robots() -> Vec<Robot> {
    let mut robots: Vec<Robot> = Vec::new();
    for i in 0..2 as usize {
      let mut input_line = String::new();
      io::stdin().read_line(&mut input_line).unwrap();
      let inputs = input_line.split(" ").collect::<Vec<_>>();
      let robot = Robot {
        target: inputs[0].trim().to_string(),
        eta: parse_input!(inputs[1], i32),
        score: parse_input!(inputs[2], i32),
        storage_a: parse_input!(inputs[3], i32),
        storage_b: parse_input!(inputs[4], i32),
        storage_c: parse_input!(inputs[5], i32),
        storage_d: parse_input!(inputs[6], i32),
        storage_e: parse_input!(inputs[7], i32),
        expertise_a: parse_input!(inputs[8], i32),
        expertise_b: parse_input!(inputs[9], i32),
        expertise_c: parse_input!(inputs[10], i32),
        expertise_d: parse_input!(inputs[11], i32),
        expertise_e: parse_input!(inputs[12], i32),
      };
      robots.push(robot);
    }
    robots
  }

  pub fn parse_samples() -> Vec<Sample> {
    let mut input_line = String::new();
    io::stdin().read_line(&mut input_line).unwrap();
    let sample_count = parse_input!(input_line, i32);
    let mut samples: Vec<Sample> = Vec::new();
    for i in 0..sample_count as usize {
      let mut input_line = String::new();
      io::stdin().read_line(&mut input_line).unwrap();
      let inputs = input_line.split(" ").collect::<Vec<_>>();
      let sample = Sample {
        sample_id: parse_input!(inputs[0], i32),
        carried_by: parse_input!(inputs[1], i32),
        rank: parse_input!(inputs[2], i32),
        expertise_gain: inputs[3].trim().to_string(),
        health: parse_input!(inputs[4], i32),
        cost_a: parse_input!(inputs[5], i32),
        cost_b: parse_input!(inputs[6], i32),
        cost_c: parse_input!(inputs[7], i32),
        cost_d: parse_input!(inputs[8], i32),
        cost_e: parse_input!(inputs[9], i32),
      };

      samples.push(sample);
    }
    samples
  }
}

pub struct Robot {
  target: String,
  eta: i32,
  score: i32,
  storage_a: i32,
  storage_b: i32,
  storage_c: i32,
  storage_d: i32,
  storage_e: i32,
  expertise_a: i32,
  expertise_b: i32,
  expertise_c: i32,
  expertise_d: i32,
  expertise_e: i32,
}

pub struct Sample {
  sample_id: i32,
  carried_by: i32,
  rank: i32,
  expertise_gain: String,
  health: i32,
  cost_a: i32,
  cost_b: i32,
  cost_c: i32,
  cost_d: i32,
  cost_e: i32,
}

impl Sample {
  pub fn is_undiagnosed(&self) -> bool {
    self.cost_a == -1
    // && self.cost_b == 0 && self.cost_c == 0 && self.cost_d == 0 && self.cost_e == 0
  }
}
