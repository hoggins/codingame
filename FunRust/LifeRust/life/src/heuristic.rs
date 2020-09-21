use std::cmp;
use std::io;
use std::ops::Add;
use std::ops::Index;
use std::ops::Sub;

macro_rules! parse_input {
    ($x:expr, $t:ident) => {
        $x.trim().parse::<$t>().unwrap()
    };
}

fn main() {
    input::parse_projects();

    let mut goals: Vec<Box<dyn Goal>> = Vec::new();
    goals.push(Box::new(GoalGoToSample::new(11)));
    goals.push(Box::new(GoalGoToDiagnosis::new(11)));
    goals.push(Box::new(GoalGoToMolecules::new(8)));
    goals.push(Box::new(GoalGoToLab::new(9)));

    goals.push(Box::new(GoalTakeSample::new(100)));
    goals.push(Box::new(GoalDiagnoseSample::new(100)));
    //goals.push(Box::new(GoalTakeMolecules::new(20)));
    goals.push(Box::new(GoalTakeMoleculesMax::new(100)));
    goals.push(Box::new(GoalCompleteSample::new(100)));

    goals.push(Box::new(GoalGoToDump::new(1)));
    goals.push(Box::new(GoalCompleteDump::new(1)));

    // game loop
    loop {
        let robots = input::parse_robots();
        let available = input::parse_available();
        let samples = input::parse_samples();
        let gs = GameState {
            robots: robots,
            samples: samples,
            available: available,
        };

        let mut best_weight = 0f64;
        let mut best_goal: Option<&Box<_>> = None;
        for goal in goals.iter_mut() {
            let weight = goal.evaluate(&gs);
            //eprintln!("{} weight {}", goal.name(), weight);

            if weight > best_weight {
                best_weight = weight;
                best_goal = Some(goal);
            }
        }

        if best_goal.is_some() {
            best_goal.unwrap().execute();
        } else {
            println!("WAIT");
        }

        // Write an action using println!("message...");
        // To debug: eprintln!("Debug message...");

        //println!("GOTO DIAGNOSIS");
    }
}

mod input {
    use crate::MoleculeSet;
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
                storage: MoleculeSet::parse(&inputs[3..=7]),
                expertise: MoleculeSet::parse(&inputs[8..=12]),
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
}

pub struct GameState {
    robots: Vec<Robot>,
    samples: Vec<Sample>,
    available: MoleculeSet,
}

impl GameState {
    fn myself(&self) -> &Robot {
        self.robots.index(0)
    }

    fn get_completed_sample(&self) -> Option<&Sample> {
        let myself = self.myself();
        self.samples.iter().find(|&x| {
            x.carried_by == 0 && x.is_diagnosed() && myself.missing_molecules(x).total() == 0
        })
    }

    fn get_best_to_collect(&self) -> Option<&Sample> {
        let myself = self.myself();
        self.samples
            .iter()
            .filter(|&x| {
                x.carried_by == 0
                    && x.is_diagnosed()
                    && myself.missing_molecules(x).total() > 0
                    && self.available.can_substract(&myself.missing_molecules(x))
                    && (myself.storage + myself.missing_molecules(x)).total() < 10
            })
            .min_by_key(|&x| myself.missing_molecules(x).total())
    }
}

pub struct Robot {
    target: String,
    eta: i32,
    score: i32,
    storage: MoleculeSet,
    expertise: MoleculeSet,
}

impl Robot {
    pub fn missing_molecules(&self, s: &Sample) -> MoleculeSet {
        ((s.cost - self.storage) - self.expertise).clamp_negative()
    }
    pub fn missing_cost(&self, s: &MoleculeSet) -> MoleculeSet {
        ((*s - self.storage) - self.expertise).clamp_negative()
    }
}

pub struct Sample {
    sample_id: i32,
    carried_by: i32,
    rank: i32,
    expertise_gain: String,
    health: i32,
    cost: MoleculeSet,
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

pub trait Goal {
    fn name(&self) -> &'static str;
    fn evaluate(&mut self, gs: &GameState) -> f64;
    fn execute(&self);
}

const SAMPLES: &str = "SAMPLES";
const DIAGNOSIS: &str = "DIAGNOSIS";
const MOLECULES: &str = "MOLECULES";
const LABORATORY: &str = "LABORATORY";

pub struct GoalTakeSample {
    base_weight: i32,
    rank: i32,
}

impl GoalTakeSample {
    fn new(w: i32) -> GoalTakeSample {
        GoalTakeSample {
            base_weight: w,
            rank: 2,
        }
    }
}

impl Goal for GoalTakeSample {
    fn name(&self) -> &'static str {
        "GoalTakeSample"
    }
    fn evaluate(&mut self, gs: &GameState) -> f64 {
        if gs.myself().target != SAMPLES {
            return 0f64;
        }

        let carring = gs.samples.iter().filter(|&x| x.carried_by == 0).count();
        if carring >= 3 {
            return 0f64;
        }

        self.rank = 2;
        /*if gs.myself().score < 3 {
          self.rank = 1;
        } else if gs.myself().score < 12 {
          let count_green = gs
            .samples
            .iter()
            .filter(|&x| x.carried_by == 0 && x.rank == 1)
            .count();
          self.rank = match count_green {
            0 => 1,
            _ => 2,
          }
        } else if gs.myself().score < 100 {
          self.rank = 2;
        } else {
          let count_red = gs
            .samples
            .iter()
            .filter(|&x| x.carried_by == 0 && x.rank == 3)
            .count();
          self.rank = match count_red {
            0 => 3,
            _ => 2,
          }
        }*/

        self.base_weight as f64
    }
    fn execute(&self) {
        println!("CONNECT {}", self.rank);
    }
}

pub struct GoalGoToSample {
    base_weight: i32,
}
impl GoalGoToSample {
    fn new(w: i32) -> GoalGoToSample {
        GoalGoToSample { base_weight: w }
    }
}

impl Goal for GoalGoToSample {
    fn name(&self) -> &'static str {
        "GoalGoToSample"
    }
    fn evaluate(&mut self, gs: &GameState) -> f64 {
        if gs.myself().target == SAMPLES {
            return 0f64;
        }

        let carring = gs.samples.iter().filter(|&x| x.carried_by == 0).count();
        if carring >= 3 {
            return 0f64;
        }

        self.base_weight as f64 - ((carring as f64) / 3f64) * self.base_weight as f64
    }
    fn execute(&self) {
        println!("GOTO SAMPLES");
    }
}

pub struct GoalGoToDiagnosis {
    base_weight: i32,
}
impl GoalGoToDiagnosis {
    fn new(w: i32) -> GoalGoToDiagnosis {
        GoalGoToDiagnosis { base_weight: w }
    }
}

impl Goal for GoalGoToDiagnosis {
    fn name(&self) -> &'static str {
        "GoalGoToDiagnosis"
    }
    fn evaluate(&mut self, gs: &GameState) -> f64 {
        if gs.myself().target == DIAGNOSIS {
            return 0f64;
        }

        let carring = gs
            .samples
            .iter()
            .filter(|&x| x.carried_by == 0 && x.is_undiagnosed())
            .count();
        if carring == 0 {
            return 0f64;
        }

        self.base_weight as f64
    }
    fn execute(&self) {
        println!("GOTO DIAGNOSIS");
    }
}

pub struct GoalDiagnoseSample {
    base_weight: i32,
    to_diagnose: Option<i32>,
}
impl GoalDiagnoseSample {
    fn new(w: i32) -> GoalDiagnoseSample {
        GoalDiagnoseSample {
            base_weight: w,
            to_diagnose: None,
        }
    }
}

impl Goal for GoalDiagnoseSample {
    fn name(&self) -> &'static str {
        "GoalDiagnoseSample"
    }
    fn evaluate(&mut self, gs: &GameState) -> f64 {
        if gs.myself().target != DIAGNOSIS {
            return 0f64;
        }

        self.to_diagnose = None;
        let to_diagnose = gs
            .samples
            .iter()
            .find(|&x| x.carried_by == 0 && x.is_undiagnosed());
        if to_diagnose.is_none() {
            return 0f64;
        }
        self.to_diagnose = Some(to_diagnose.unwrap().sample_id);

        self.base_weight as f64
    }
    fn execute(&self) {
        println!("CONNECT {}", self.to_diagnose.unwrap());
    }
}

pub struct GoalGoToMolecules {
    base_weight: i32,
}
impl GoalGoToMolecules {
    fn new(w: i32) -> GoalGoToMolecules {
        GoalGoToMolecules { base_weight: w }
    }
}

impl Goal for GoalGoToMolecules {
    fn name(&self) -> &'static str {
        "GoalGoToMolecules"
    }
    fn evaluate(&mut self, gs: &GameState) -> f64 {
        if gs.myself().target == MOLECULES {
            return 0f64;
        }

        if gs.get_completed_sample().is_some() {
            return 0f64;
        }

        if gs.get_best_to_collect().is_none() {
            return 0f64;
        }

        let myself = gs.myself();
        let carring = gs
            .samples
            .iter()
            .filter(|&x| {
                x.carried_by == 0
                    && x.is_diagnosed()
                    && gs.available.can_substract(&myself.missing_molecules(x))
            })
            .count();
        ((carring as f64) / 3f64) * self.base_weight as f64
    }
    fn execute(&self) {
        println!("GOTO {}", MOLECULES);
    }
}

pub struct GoalTakeMolecules {
    base_weight: i32,
    molecule: Option<String>,
}
impl GoalTakeMolecules {
    fn new(w: i32) -> GoalTakeMolecules {
        GoalTakeMolecules {
            base_weight: w,
            molecule: None,
        }
    }
}

impl Goal for GoalTakeMolecules {
    fn name(&self) -> &'static str {
        "GoalTakeMolecules"
    }
    fn evaluate(&mut self, gs: &GameState) -> f64 {
        self.molecule = None;
        let myself = gs.myself();
        if myself.target != MOLECULES {
            return 0f64;
        }

        let best = gs.get_best_to_collect();
        if let Some(sample) = best {
            self.molecule = Some(myself.missing_molecules(sample).first_name().to_string());
            return self.base_weight as f64;
        }

        if gs.get_completed_sample().is_some() {
            return 0f64;
        }

        eprintln!("no sample to fullfill");

        eprintln!("available {:?}", gs.available);
        eprintln!("storage {:?}", myself.storage);
        eprintln!("expertise {:?}", myself.expertise);

        for s in gs.samples.iter().filter(|&x| x.carried_by == 0) {
            eprintln!("cost {:?}", s.cost);
            eprintln!("missing {:?}", myself.missing_molecules(&s));
            eprintln!("dif {:?}", gs.available - myself.missing_molecules(&s));
            eprintln!("");
        }

        //panic!();
        0f64
    }
    fn execute(&self) {
        println!("CONNECT {}", self.molecule.as_ref().unwrap());
    }
}

pub struct GoalTakeMoleculesMax {
    base_weight: i32,
    molecule: Option<&'static str>,
    variations: Vec<Vec<i32>>,
}
impl GoalTakeMoleculesMax {
    fn new(w: i32) -> GoalTakeMoleculesMax {
        GoalTakeMoleculesMax {
            base_weight: w,
            molecule: None,
            variations: vec![
                vec![1, 0, 0],
                vec![1, 2, 0],
                vec![1, 2, 3],
                vec![1, 0, 3],
                vec![0, 2, 3],
                vec![0, 2, 0],
                vec![0, 0, 3],
            ],
        }
    }

    fn take<'a>(idx: usize, options: &Vec<&'a Sample>) -> Option<&'a Sample> {
        if idx == 0 || options.len() <= idx {
            return None;
        }
        let idx = idx - 1;
        Some(options.index(idx))
    }

    fn score_set(
        &self,
        idx: usize,
        gs: &GameState,
        options: &Vec<&Sample>,
    ) -> (i32, Option<MoleculeSet>) {
        let layout = self.variations.index(idx);
        let mut total = MoleculeSet::new();
        let mut score = 0;

        let option_f = GoalTakeMoleculesMax::take(layout[0] as usize, &options);
        if let Some(sample) = option_f {
            total = total + sample.cost;
            score += sample.health;
        }
        let option_s = GoalTakeMoleculesMax::take(layout[1] as usize, &options);
        if let Some(sample) = option_s {
            total = total + sample.cost;
            score += sample.health;
        }
        let option_t = GoalTakeMoleculesMax::take(layout[2] as usize, &options);
        if let Some(sample) = option_t {
            total = total + sample.cost;
            score += sample.health;
        }
        let missing = gs.myself().missing_cost(&total);
        if !gs.available.can_substract(&missing) || (gs.myself().storage + missing).total() > 10 {
            return (0, None);
        }
        (score, Some(total))
    }
}

impl Goal for GoalTakeMoleculesMax {
    fn name(&self) -> &'static str {
        "GoalTakeMoleculesMax"
    }
    fn evaluate(&mut self, gs: &GameState) -> f64 {
        self.molecule = None;
        let myself = gs.myself();
        if myself.target != MOLECULES {
            return 0f64;
        }

        let options = gs
            .samples
            .iter()
            .filter(|&x| x.carried_by == 0 && x.is_diagnosed())
            .collect::<Vec<&Sample>>();

        let mut best_score = 0;
        let mut best_mols: Option<MoleculeSet> = None;

        for idx in 0..7 {
            let (score, total) = self.score_set(idx, &gs, &options);
            if best_score > score {
                continue;
            }
            best_score = score;
            best_mols = total;
        }

        if let Some(best_mols) = best_mols {
            let missing = myself.missing_cost(&best_mols);
            eprintln!("best {} {:?}", best_score, missing);
            if missing.total() == 0 {
                return 0f64;
            }
            self.molecule = Some(missing.first_name());
            return self.base_weight as f64;
        }

        0f64
    }

    fn execute(&self) {
        println!("CONNECT {}", self.molecule.unwrap());
    }
}

pub struct GoalGoToLab {
    base_weight: i32,
}
impl GoalGoToLab {
    fn new(w: i32) -> GoalGoToLab {
        GoalGoToLab { base_weight: w }
    }
}

impl Goal for GoalGoToLab {
    fn name(&self) -> &'static str {
        "GoalGoToLab"
    }
    fn evaluate(&mut self, gs: &GameState) -> f64 {
        if gs.myself().target == LABORATORY {
            return 0f64;
        }

        if gs.get_completed_sample().is_none() {
            return 0f64;
        }

        self.base_weight as f64
    }

    fn execute(&self) {
        println!("GOTO {}", LABORATORY);
    }
}

pub struct GoalCompleteSample {
    base_weight: i32,
    sample_id: Option<i32>,
}
impl GoalCompleteSample {
    fn new(w: i32) -> GoalCompleteSample {
        GoalCompleteSample {
            base_weight: w,
            sample_id: None,
        }
    }
}

impl Goal for GoalCompleteSample {
    fn name(&self) -> &'static str {
        "GoalCompleteSample"
    }
    fn evaluate(&mut self, gs: &GameState) -> f64 {
        self.sample_id = None;
        let myself = gs.myself();
        if myself.target != LABORATORY {
            return 0f64;
        }

        if let Some(completed) = gs.get_completed_sample() {
            self.sample_id = Some(completed.sample_id);
            return self.base_weight as f64;
        }

        0f64
    }
    fn execute(&self) {
        println!("CONNECT {}", self.sample_id.as_ref().unwrap());
    }
}

pub struct GoalGoToDump {
    base_weight: i32,
}
impl GoalGoToDump {
    fn new(w: i32) -> GoalGoToDump {
        GoalGoToDump { base_weight: w }
    }
}

impl Goal for GoalGoToDump {
    fn name(&self) -> &'static str {
        "GoalGoToDump"
    }
    fn evaluate(&mut self, gs: &GameState) -> f64 {
        if gs.myself().target == DIAGNOSIS {
            return 0f64;
        }

        let carring = gs.samples.iter().filter(|&x| x.carried_by == 0).count();
        if carring < 3 {
            return 0f64;
        }

        if gs.get_completed_sample().is_some() {
            return 0f64;
        }

        if gs.get_best_to_collect().is_some() {
            eprintln!("has best, no dump");
            let myself = gs.myself();
            eprintln!("available {:?}", gs.available);
            eprintln!("storage {:?}", myself.storage);
            eprintln!("expertise {:?}", myself.expertise);
            for s in gs.samples.iter().filter(|&x| x.carried_by == 0) {
                eprintln!("cost {:?}", s.cost);
                eprintln!("missing {:?}", myself.missing_molecules(&s));
                eprintln!("dif {:?}", gs.available - myself.missing_molecules(&s));
                eprintln!("");
            }
            return 0f64;
        }

        self.base_weight as f64
    }

    fn execute(&self) {
        println!("GOTO {}", DIAGNOSIS);
    }
}

pub struct GoalCompleteDump {
    base_weight: i32,
    sample_id: Option<i32>,
}
impl GoalCompleteDump {
    fn new(w: i32) -> GoalCompleteDump {
        GoalCompleteDump {
            base_weight: w,
            sample_id: None,
        }
    }
}

impl Goal for GoalCompleteDump {
    fn name(&self) -> &'static str {
        "GoalCompleteDump"
    }
    fn evaluate(&mut self, gs: &GameState) -> f64 {
        self.sample_id = None;
        let myself = gs.myself();
        if myself.target != DIAGNOSIS {
            return 0f64;
        }

        let carring = gs
            .samples
            .iter()
            .filter(|&x| x.carried_by == 0 && x.is_diagnosed())
            .count();
        if carring < 3 {
            return 0f64;
        }

        if gs.get_best_to_collect().is_some() {
            return 0f64;
        }

        let worse = gs
            .samples
            .iter()
            .filter(|&x| x.carried_by == 0 && x.is_diagnosed())
            .max_by_key(|&x| (gs.available - myself.missing_molecules(x)).abs().total());

        if let Some(completed) = worse {
            self.sample_id = Some(completed.sample_id);
            return self.base_weight as f64;
        }

        0f64
    }
    fn execute(&self) {
        println!("CONNECT {}", self.sample_id.as_ref().unwrap());
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
    fn new() -> MoleculeSet {
        MoleculeSet {
            a: 0,
            b: 0,
            c: 0,
            d: 0,
            e: 0,
        }
    }

    fn parse(inputs: &[&str]) -> MoleculeSet {
        MoleculeSet {
            a: parse_input!(inputs[0], i32),
            b: parse_input!(inputs[1], i32),
            c: parse_input!(inputs[2], i32),
            d: parse_input!(inputs[3], i32),
            e: parse_input!(inputs[4], i32),
        }
    }
    fn clamp_negative(self) -> MoleculeSet {
        MoleculeSet {
            a: cmp::max(0, self.a),
            b: cmp::max(0, self.b),
            c: cmp::max(0, self.c),
            d: cmp::max(0, self.d),
            e: cmp::max(0, self.e),
        }
    }
    fn abs(self) -> MoleculeSet {
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
