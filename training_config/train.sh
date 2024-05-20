run_id="ant_100env_1area_100ts_cuda_threaded"
num_envs="1" # must be 1 if env path is not specified
num_areas="100"
time_scale="100"
no_graphics="False"
torch_device="cuda"
# training_params="../Assets/config/ant_config.yaml --run-id=$run_id --num-envs=$num_envs --num-areas=$num_areas --time-scale=$time_scale --no-graphics=$no_graphics --torch-device=$torch_device --threaded=True"
training_params="../Assets/config/ant_config.yaml --run-id=$run_id --num-envs=$num_envs --num-areas=$num_areas --time-scale=$time_scale --torch-device=$torch_device "


echo $training_params
mlagents-learn $training_params
