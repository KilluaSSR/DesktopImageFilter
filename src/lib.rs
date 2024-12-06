use std::fs;
use std::fs::File;
use std::io::BufReader;
use std::path::Path;
use jpeg_decoder::Decoder;
use walkdir::WalkDir;

pub struct Config{
    pub origin_dir: String,
    pub destination_dir: String,
    pub ratio: f32
}
impl Config {
    pub fn new(args: &[String])->Result<Config, &'static str>{
        if args.len()<4 {
            return Err("Not enough arguments");
        }
        let origin = args[1].clone();
        let destination = args[2].clone();
        let ratio = args[3].clone().parse::<f32>().unwrap();
        Ok(Config { origin_dir: origin,destination_dir: destination ,ratio })
    }
}
pub fn run(config: Config) {
    let src_dir = config.origin_dir;
    let dest_dir = config.destination_dir;
    let aspect_ratio_threshold = config.ratio;
    check_exists(&dest_dir);
    for entry in WalkDir::new(src_dir).into_iter().filter_map(|e| e.ok()) {
        let path = entry.path();
        if path.extension().and_then(|ext| ext.to_str()) == Some("jpg") {
            println!("Processing file: {:?}", path);
            let file = match File::open(path) {
                Ok(file) => file,
                Err(_) => {
                    eprintln!("Failed to open image file: {:?}", path);
                    continue;
                }
            };
            let mut decoder = Decoder::new(BufReader::new(file));
            if let Err(_) = decoder.read_info() {
                eprintln!("Failed to read image metadata: {:?}", path);
                continue;
            }
            let metadata = match decoder.info() {
                Some(info) => info,
                None => {
                    eprintln!("Failed to get image metadata: {:?}", path);
                    continue;
                }
            };
            let width = metadata.width as f32;
            let height = metadata.height as f32;
            let aspect_ratio = width / height;

            if aspect_ratio > aspect_ratio_threshold {
                let file_name = path.file_name().unwrap();
                let dest_path = Path::new(&dest_dir).join(file_name);
                if let Err(_) = fs::rename(path, dest_path) {
                    eprintln!("Failed to move file: {:?}", path);
                } else {
                    println!("Moved: {:?}", file_name);
                }
            }
        }
    }
}
fn check_exists(dir: &str){
    if !Path::new(dir).exists(){
        fs::create_dir(dir).expect("Failed to create destination directory")
    }
}