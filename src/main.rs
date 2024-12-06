
use std::env::args;
use DesktopImageFilter::{run, Config};
fn main() {
    let args: Vec<String> = args().collect();
    let config = Config::new(&args).expect("ERROR");
    run(config)
}