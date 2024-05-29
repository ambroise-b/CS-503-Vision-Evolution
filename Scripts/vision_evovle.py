import json
import os
# -- coding: utf-8 --
def run_train(eyes_range, fov_range, config_file='./ant_config.yaml', eye_start_idx=9, fov_start_idx=0, max_steps=20):

  running_eyes_nb = eyes_range[eye_start_idx]
  running_fov = fov_range[fov_start_idx]

  running_eyes_nb_idx = eye_start_idx
  running_fov_idx = fov_start_idx

  runs = {}

  for eyes_nb in eyes_range:
    runs[eyes_nb] = {}
    for fov in fov_range:
      runs[eyes_nb][fov] = {'tested' : False, 'cum_reward_mean' : -1000, 'ep_lenght_mean' : 0}


  # fov
  # ^
  # |
  # |
  # +--------> nb_eyes

  total_steps = 0
  exec_name = 'ant_run_final'

  while total_steps < max_steps:

    current_runs = []

    #down --> fov
    if min(fov_range) != running_fov and not(runs[eyes_range[running_eyes_nb_idx]][fov_range[running_fov_idx-1]]['tested']):
      current_runs.append({'eyes_nb' : eyes_range[running_eyes_nb_idx], 'fov' : fov_range[running_fov_idx-1]})

    #up --> fov
    if max(fov_range) != running_fov and not(runs[eyes_range[running_eyes_nb_idx]][fov_range[running_fov_idx+1]]['tested']):
      current_runs.append({'eyes_nb' : eyes_range[running_eyes_nb_idx], 'fov' : fov_range[running_fov_idx+1]})

    #left --> nb_eyes
    if min(eyes_range) != running_eyes_nb and not(runs[eyes_range[running_eyes_nb_idx-1]][fov_range[running_fov_idx]]['tested']):
      current_runs.append({'eyes_nb' : eyes_range[running_eyes_nb_idx-1], 'fov' : fov_range[running_fov_idx]})

    # right ---> nb_eyes
    if max(eyes_range) != running_eyes_nb and not(runs[eyes_range[running_eyes_nb_idx+1]][fov_range[running_fov_idx]]['tested']):
      current_runs.append({'eyes_nb' : eyes_range[running_eyes_nb_idx+1], 'fov' : fov_range[running_fov_idx]})

    autorun_id = 0
    #do this at most 4 times
    for run_to_do in current_runs:
      run_name = f'autorun_{autorun_id}fov{run_to_do["fov"]}nbeyes{run_to_do["eyes_nb"]}'

      #run trainging
      print(f'---running with nb_eyes={run_to_do["eyes_nb"]}, fov={run_to_do["fov"]}---')

      out_cmd = os.popen(f'mlagents-learn {config_file}  --env={exec_name} --run-id={run_name} --no-graphics --env-args --fov {run_to_do["fov"]} --nbRays {run_to_do["eyes_nb"]}').read()


      with open(f'./results/{run_name}/run_logs/timers.json') as timer_js:
        timers_data = json.load(timer_js)
        runs[run_to_do["eyes_nb"]][run_to_do["fov"]]['tested'] = True
        runs[run_to_do["eyes_nb"]][run_to_do["fov"]]['ep_lenght_mean'] =  timers_data['gauges']['Ant.Environment.EpisodeLength.mean']['value']
        runs[run_to_do["eyes_nb"]][run_to_do["fov"]]['cum_reward_mean'] = timers_data['gauges']['Ant.Environment.CumulativeReward.mean']['value']
      autorun_id +=1


    #take the min values
    current_cum_reward = -1000
    max_eyes_nb = 0
    max_eyes_nb_idx = -1
    max_fov = 0
    max_fov_idx = -1


    for eye_i, eyes_nb in enumerate(eyes_range):
      for fov_i, fov in enumerate(fov_range):
        if runs[eyes_nb][fov]['cum_reward_mean'] > current_cum_reward:
          current_cum_reward = runs[eyes_nb][fov]['cum_reward_mean']
          max_eyes_nb = eyes_nb
          max_eyes_nb_idx = eye_i
          max_fov = fov
          max_fov_idx = fov_i



    running_eyes_nb = max_eyes_nb
    running_fov = max_fov

    running_eyes_nb_idx = max_eyes_nb_idx
    running_fov_idx = max_fov_idx


    if len(current_runs) == 0:
      print('lowest value found')
      return runs

    total_steps += 1

  print('Max steps reached')
  return runs


def write_runs(runs, run_name):

  with open(f'{run_name}.json', 'w') as json_file:
      json.dump(runs, json_file, indent=4)

eyes_range = [i for i in range(20) if i % 2 == 1] #[1, 3, 5, 7, 9, 11, 13, 15, 17, 19]
fov_range = [i for i in range(15, 181, 15)] #[15, 30, 45, 60, 75, 90, 105, 120, 135, 150, 165, 180]

runs = run_train(eyes_range, fov_range, config_file='./ant_config.yaml')
write_runs(runs, 'final_output.json')