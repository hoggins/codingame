use crate::entities::*;

macro_rules! parse_input {
  ($x:expr, $t:ident) => {
    $x.trim().parse::<$t>().unwrap()
  };
}

const SAMPLES: &str = "SAMPLES";
const DIAGNOSIS: &str = "DIAGNOSIS";
const MOLECULES: &str = "MOLECULES";
const LABORATORY: &str = "LABORATORY";
const START_POS: &str = "START_POS";
const MOLECULE_A: &str = "A";
const MOLECULE_B: &str = "B";
const MOLECULE_C: &str = "C";
const MOLECULE_D: &str = "D";
const MOLECULE_E: &str = "E";
const MOLECULE_0: &str = "0";

const NEG_INFINITY: f64 = std::f64::NEG_INFINITY;
const INFINITY: f64 = std::f64::INFINITY;

static mut FAKE_SAMPLE_ID: i32 = 5000;

fn main() {
  input::parse_projects();
  // game loop
  loop {
    let robots = input::parse_robots();
    let available = input::parse_available();
    let samples = input::parse_samples();
    let mut gs = GameState {
      robots: robots,
      samples: samples,
      available: available,
    };

    let action = minimax::decide_move(&mut gs);

    if let Some(action) = action {
      action.execute();
    } else {
      println!("WAIT");
    }

    // Write an action using println!("message...");
    // To debug: eprintln!("Debug message...");

    //println!("GOTO DIAGNOSIS");
  }
}

mod entities {
  use std::cmp;
  use std::ops::Add;
  use std::ops::Sub;

  pub struct GameState {
    pub robots: Vec<Robot>,
    pub samples: Vec<Sample>,
    pub available: MoleculeSet,
  }

  pub struct Robot {
    pub target: &'static str,
    pub eta: i32,
    pub score: i32,
    pub storage: MoleculeSet,
    pub expertise: MoleculeSet,
  }

  impl Robot {
    pub fn missing_cost(&self, s: &MoleculeSet) -> MoleculeSet {
      ((*s - self.storage) - self.expertise).clamp_negative()
    }
  }

  pub struct Sample {
    pub sample_id: i32,
    pub carried_by: i32,
    pub rank: i32,
    pub expertise_gain: &'static str,
    pub health: i32,
    pub cost: MoleculeSet,
  }

  impl Sample {
    pub fn is_undiagnosed(&self) -> bool {
      self.cost.a == -1
      // && self.cost_b == 0 && self.cost_c == 0 && self.cost_d == 0 && self.cost_e == 0
    }
    pub fn is_diagnosed(&self) -> bool {
      self.cost.a >= 0
    }
  }

  #[derive(Debug, Clone, Copy)]
  pub struct MoleculeSet {
    a: i32,
    b: i32,
    c: i32,
    d: i32,
    e: i32,
  }

  impl MoleculeSet {
    pub fn new(val: i32) -> MoleculeSet {
      MoleculeSet {
        a: val,
        b: val,
        c: val,
        d: val,
        e: val,
      }
    }

    pub fn parse(inputs: &[&str]) -> MoleculeSet {
      MoleculeSet {
        a: parse_input!(inputs[0], i32),
        b: parse_input!(inputs[1], i32),
        c: parse_input!(inputs[2], i32),
        d: parse_input!(inputs[3], i32),
        e: parse_input!(inputs[4], i32),
      }
    }
    pub fn clamp_negative(self) -> MoleculeSet {
      MoleculeSet {
        a: cmp::max(0, self.a),
        b: cmp::max(0, self.b),
        c: cmp::max(0, self.c),
        d: cmp::max(0, self.d),
        e: cmp::max(0, self.e),
      }
    }
    pub fn abs(self) -> MoleculeSet {
      MoleculeSet {
        a: self.a.abs(),
        b: self.b.abs(),
        c: self.c.abs(),
        d: self.d.abs(),
        e: self.e.abs(),
      }
    }

    fn total(&self) -> i32 {
      self.a + self.b + self.c + self.d + self.e
    }

    fn can_substract(&self, other: &MoleculeSet) -> bool {
      self.a >= other.a
        && self.b >= other.b
        && self.c >= other.c
        && self.d >= other.d
        && self.e >= other.e
    }

    fn first_name(&self) -> &'static str {
      if self.a > 0 {
        return "A";
      }
      if self.b > 0 {
        return "B";
      }
      if self.c > 0 {
        return "C";
      }
      if self.d > 0 {
        return "D";
      }
      if self.e > 0 {
        return "E";
      }
      "U"
    }
  }

  impl Add for MoleculeSet {
    type Output = MoleculeSet;
    fn add(self, other: MoleculeSet) -> <Self as std::ops::Add<MoleculeSet>>::Output {
      MoleculeSet {
        a: self.a + other.a,
        b: self.b + other.b,
        c: self.c + other.c,
        d: self.d + other.d,
        e: self.e + other.e,
      }
    }
  }

  impl Sub for MoleculeSet {
    type Output = MoleculeSet;
    fn sub(self, other: MoleculeSet) -> <Self as std::ops::Sub<MoleculeSet>>::Output {
      MoleculeSet {
        a: self.a - other.a,
        b: self.b - other.b,
        c: self.c - other.c,
        d: self.d - other.d,
        e: self.e - other.e,
      }
    }
  }
}

mod input {
  use crate::entities::*;
  use crate::*;
  use std::io;
  pub fn parse_robots() -> Vec<Robot> {
    let mut robots: Vec<Robot> = Vec::new();
    for _ in 0..2 as usize {
      let mut input_line = String::new();
      io::stdin().read_line(&mut input_line).unwrap();
      let inputs = input_line.split(" ").collect::<Vec<_>>();
      let robot = Robot {
        target: parse_target(inputs[0].trim()),
        eta: parse_input!(inputs[1], i32),
        score: parse_input!(inputs[2], i32),
        storage: MoleculeSet::parse(&inputs[3..=7]),
        expertise: MoleculeSet::parse(&inputs[8..=12]),
      };
      robots.push(robot);
    }
    robots
  }

  fn parse_target<'a>(input: &'a str) -> &'static str {
    match input {
      LABORATORY => LABORATORY,
      DIAGNOSIS => DIAGNOSIS,
      SAMPLES => SAMPLES,
      MOLECULES => MOLECULES,
      START_POS => START_POS,
      _ => panic!("val : {}", input),
    }
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
        expertise_gain: parse_molecule(inputs[3].trim()),
        health: parse_input!(inputs[4], i32),
        cost: MoleculeSet::parse(&inputs[5..=9]),
      };

      samples.push(sample);
    }
    samples
  }

  pub fn parse_available() -> MoleculeSet {
    let mut input_line = String::new();
    io::stdin().read_line(&mut input_line).unwrap();
    let inputs = input_line.split(" ").collect::<Vec<_>>();
    MoleculeSet::parse(inputs.as_slice())
  }

  pub fn parse_projects() {
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
  }

  fn parse_molecule<'a>(input: &'a str) -> &'static str {
    match input {
      MOLECULE_A => MOLECULE_A,
      MOLECULE_B => MOLECULE_B,
      MOLECULE_C => MOLECULE_C,
      MOLECULE_D => MOLECULE_D,
      MOLECULE_E => MOLECULE_E,
      MOLECULE_0 => MOLECULE_0,
      _ => panic!("val : {}", input),
    }
  }
}

mod actions {
  use crate::entities::*;
  use crate::*;
  use std::ops::IndexMut;

  macro_rules! implement_name {
    () => {
      fn name(&self) -> &'static str {
        type_name(self)
      }
    };
  }

  fn type_name<T>(_: &T) -> &'static str {
    std::any::type_name::<T>()
  }

  pub trait Action {
    fn name(&self) -> &'static str;
    fn apply(&mut self, gs: &mut GameState);
    fn undo(&mut self, gs: &mut GameState);
    fn execute(&self);
  }

  /*
   * ActionGoTo
   */
  pub struct ActionGoTo {
    robot_idx: usize,
    initial_loc: &'static str,
    target_loc: &'static str,
  }

  impl ActionGoTo {
    pub fn new(robot_idx: usize, target_loc: &'static str) -> ActionGoTo {
      ActionGoTo {
        robot_idx: robot_idx,
        initial_loc: "",
        target_loc: target_loc,
      }
    }
  }

  impl Action for ActionGoTo {
    implement_name!();
    fn apply(&mut self, gs: &mut GameState) {
      let mut robot = gs.robots.index_mut(self.robot_idx);
      self.initial_loc = robot.target;
      robot.target = self.target_loc;
    }
    fn undo(&mut self, gs: &mut GameState) {
      let mut robot = gs.robots.index_mut(self.robot_idx);
      robot.target = self.initial_loc;
    }
    fn execute(&self) {
      println!("GOTO {}", self.target_loc);
    }
  }

  /*
   * ActionTakeSample
   */
  pub struct ActionTakeSample {
    robot_idx: usize,
    generated_sample_id: Option<i32>,
  }

  impl ActionTakeSample {
    pub fn new(robot_idx: usize) -> ActionTakeSample {
      ActionTakeSample {
        robot_idx: robot_idx,
        generated_sample_id: None,
      }
    }
    unsafe fn next_id() -> i32 {
      let ret = FAKE_SAMPLE_ID;
      FAKE_SAMPLE_ID += 1;
      ret
    }
  }

  impl Action for ActionTakeSample {
    implement_name!();
    fn apply(&mut self, gs: &mut GameState) {
      let mut robot = gs.robots.index_mut(self.robot_idx);
      let mut sample = Sample {
        sample_id: unsafe { ActionTakeSample::next_id() },
        carried_by: self.robot_idx as i32,
        rank: 2,
        expertise_gain: MOLECULE_0,
        health: 20,
        cost: MoleculeSet::new(-1),
      };
      self.generated_sample_id = Some(sample.sample_id);
      gs.samples.push(sample);
    }
    fn undo(&mut self, gs: &mut GameState) {
      if let Some(generated_sample_id) = self.generated_sample_id {
        gs.samples.retain(|x| x.sample_id != generated_sample_id);
      }
    }
    fn execute(&self) {
      println!("CONNECT 2");
    }
  }

  /*
   * ActionDiagnoseSample
   */
  pub struct ActionDiagnoseSample<'a> {
    robot: &'a Robot,
    generated_sample: Option<&'a Sample>,
  }

  impl ActionDiagnoseSample<'_> {
    fn new<'a>(robot: &'a Robot, target_loc: &'static str) -> ActionDiagnoseSample<'a> {
      ActionDiagnoseSample {
        robot: robot,
        generated_sample: None,
      }
    }
  }

  impl Action for ActionDiagnoseSample<'_> {
    implement_name!();
    fn apply(&mut self, _: &mut GameState) {}
    fn undo(&mut self, _: &mut GameState) {}
    fn execute(&self) {}
  }

  /*
   * ActionTakeMolecule
   */
  pub struct ActionTakeMolecule<'a> {
    robot: &'a Robot,
    molecule: &'static str,
  }

  impl ActionTakeMolecule<'_> {
    fn new<'a>(robot: &'a Robot, molecule: &'static str) -> ActionTakeMolecule<'a> {
      ActionTakeMolecule {
        robot: robot,
        molecule: molecule,
      }
    }
  }

  impl Action for ActionTakeMolecule<'_> {
    implement_name!();
    fn apply(&mut self, _: &mut GameState) {}
    fn undo(&mut self, _: &mut GameState) {}
    fn execute(&self) {}
  }

  /*
   * ActionTakeMolecule
   */
  pub struct ActionCompleteSample<'a> {
    robot: &'a Robot,
    sample: &'a Sample,
  }

  impl ActionCompleteSample<'_> {
    fn new<'a>(robot: &'a Robot, sample: &'a Sample) -> ActionCompleteSample<'a> {
      ActionCompleteSample {
        robot: robot,
        sample: sample,
      }
    }
  }

  impl Action for ActionCompleteSample<'_> {
    implement_name!();
    fn apply(&mut self, _: &mut GameState) {}
    fn undo(&mut self, _: &mut GameState) {}
    fn execute(&self) {}
  }
}

mod minimax {
  use crate::actions::*;
  use crate::entities::*;
  use crate::*;
  use std::cmp;
  use std::ops::IndexMut;

  struct Variation {
    score: f64,
    moves: [Vec<Box<dyn Action>>; 2],
    next_move: Option<[usize; 2]>,
  }

  impl Variation {
    fn new(score: f64, actions: Option<[Vec<Box<dyn Action>>; 2]>) -> Variation {
      Variation {
        score: score,
        moves: match actions {
          Some(x) => x,
          _ => [vec![], vec![]],
        },
        next_move: None,
      }
    }

    fn push_actions(&mut self, action_p0: Box<dyn Action>, action_p1: Box<dyn Action>) {
      self.moves[0].push(action_p0);
      self.moves[1].push(action_p1);
    }
  }

  pub fn decide_move(gs: &mut GameState) -> Option<Box<dyn Action>> {
    let mut variation = minimax(gs, 0, 10, NEG_INFINITY, INFINITY);
    let len = variation.moves[0].len();
    match len {
      0 => None,
      _ => Some(variation.moves[0].swap_remove(len - 1)),
    }
  }

  fn minimax<'a>(
    gs: &mut GameState,
    depth: i32,
    max_depth: i32,
    alpha: f64,
    beta: f64,
  ) -> Variation {
    if depth == max_depth {
      return Variation::new(evaluate(gs), None);
    }
    let mut alpha = alpha;
    let mut branch_p1 = possible_moves(gs, 0);
    let mut branch_p2 = possible_moves(gs, 1);

    let mut best_var = Variation::new(NEG_INFINITY, None);

    for mv_idx in 0..branch_p1.len() {
      let mv = branch_p1.index_mut(mv_idx);
      let mut best_var_2 = Variation::new(INFINITY, None);
      let mut local_beta = beta;
      for mv_2_idx in 0..branch_p2.len() {
        let mv_2 = branch_p2.index_mut(mv_2_idx);
        apply_action(gs, mv, mv_2);
        let var = minimax(gs, depth + 1, max_depth, alpha, local_beta);
        undo_actions(gs, mv, mv_2);
        local_beta = local_beta.min(var.score);
        if var.score < best_var_2.score {
          best_var_2 = var;
          best_var_2.next_move = Some([mv_idx, mv_2_idx]);
        }
        if local_beta <= alpha {
          break;
        }
      }
      alpha = alpha.max(best_var_2.score);
      if best_var_2.score > best_var.score {
        best_var = best_var_2;
      }
      if beta < alpha {
        break;
      }
    }

    if let Some(next_move) = best_var.next_move {
      let mv = branch_p1.swap_remove(next_move[0]);
      let mv_2 = branch_p2.swap_remove(next_move[1]);
      best_var.push_actions(mv, mv_2);
      best_var.next_move = None;
    } else {
      return Variation::new(evaluate(gs), None);
    }

    best_var
  }

  fn print_variation(var: &Variation) {
    for m in var.moves[0].iter().rev() {
      eprint!("{} ->", m.name());
    }
  }

  fn evaluate(gs: &GameState) -> f64 {
    let score = gs.samples.len() as f64;
    eprintln!("score: {}", score);
    score
  }

  fn possible_moves(gs: &GameState, robot_idx: usize) -> Vec<Box<dyn Action>> {
    let robot = &gs.robots[robot_idx];
    let mut res: Vec<Box<dyn Action>> = Vec::new();
    match robot.target {
      SAMPLES => {
        let carry_samples = gs
          .samples
          .iter()
          .filter(|&x| x.carried_by == robot_idx as i32)
          .count();
        eprint!("carry {}", carry_samples);
        if carry_samples < 3 {
          res.push(Box::new(ActionTakeSample::new(robot_idx)));
        }
      }
      _ => {
        res.push(Box::new(ActionGoTo::new(robot_idx, SAMPLES)));
      }
    };
    res
  }

  fn apply_action(gs: &mut GameState, mv: &mut Box<dyn Action>, mv_2: &mut Box<dyn Action>) {
    mv.apply(gs);
    mv_2.apply(gs);
  }
  fn undo_actions(gs: &mut GameState, mv: &mut Box<dyn Action>, mv_2: &mut Box<dyn Action>) {
    mv.undo(gs);
    mv_2.undo(gs);
  }
}
