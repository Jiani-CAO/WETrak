# WETrak
This is the repo for IEEE Transactions on Mobile Computing (TMC) 2024 paper: "Finger Tracking Using Wrist-Worn EMG Sensors".

Authors: Jiani Cao, Yang Liu, Lixiang Han, Zhenjiang Li

**Project website:** <a href="https://jiani-cao.github.io/WETrak.html"> WETrak</a>

**Demo video:**

<div align="center">       
    <a href="https://youtu.be/9CdIGp4eFiA">      
        <img src="http://img.youtube.com/vi/9CdIGp4eFiA/0.jpg"        
             alt="Everything Is AWESOME"        
             style="width:50%;">       
    </a>     
</div>

<br>

# Requirements

The program has been tested in the following environment: 

* Python 3.8.13
* Numpy 1.22.4
* Scipy 1.7.3
* Pytorch 1.12.1+cu116

# Project Structure

```
|-- tracker_network                   
    |-- data_sources					  
    	|-- data_utils.ipynb               // helper functions used to process data
        |-- finger_data.ipynb             // Pytorch Dataset prepared for train and inference
        |-- preprocess_data.ipynb	  // Setp 1: pre-process data
    |-- graph		
        |-- loss.ipynb                // loss functions used to train tracker network
    	|-- network.ipynb             // backbone of tracker network
    |-- utils
    	|-- model_utils.ipynb	          // helper functions used to load and save model
    |-- train.ipynb	                  // Step 2: main workflow of train
    |-- inference.ipynb	                  // Step 3: main workflow of inference

|-- branch_adapter   
    |-- classification                      // classification network used to classify min,max values to bins
        |-- classification_data_utils.ipynb
        |-- classification_data.ipynb
        |-- classification_model_utils.ipynb
        |-- classification_loss.ipynb
        |-- classification_network.ipynb
        |-- classification_train.ipynb       // Step 2: main workflow of train
        |-- classification_cascade_inference.ipynb
    |-- regression                           // regression network used to predict min,max values
        |-- regression_data_utils.ipynb
        |-- regression_data.ipynb
        |-- regression_model_utils.ipynb
        |-- regression_loss.ipynb
        |-- regression_network.ipynb
        |-- regression_train.ipynb          // Step 2: main workflow of train
        |-- regression_inference.ipynb

|-- Database                             

|-- Outputs                         

|-- config.ini                           // config parameters for training network
```

# Quick Start

* Pre-process the data: run `tracker_network -> data_sources -> preprocess_data.ipynb`.

* Train tracker network and branch adapter: run `tracker_network -> train.ipynb`, `branch_adapter -> classification -> classification_train.ipynb` and `branch_adapter -> regression -> regression_train.ipynb`.

* Get the tracking result: run `tracker_network -> inference.ipynb`.

  ---

# Citation

If you find our work useful in your research, please consider citing:

```
@article{cao2024finger,
  title={Finger Tracking Using Wrist-Worn EMG Sensors},
  author={Cao, Jiani and Liu, Yang and Han, Lixiang and Li, Zhenjiang},
  journal={IEEE Transactions on Mobile Computing},
  year={2024},
  publisher={IEEE}
}
```

# License

You may use this source code for academic and research purposes. Commercial use is strictly prohibited.