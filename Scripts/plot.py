import json
import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt

file_path = 'final_output.json.json'
with open(file_path, 'r') as file:
    data = json.load(file)

# Transforming the data into a DataFrame
rows = []
for eyes, fov_data in data.items():
    for fov, results in fov_data.items():
        rows.append({
            'Eyes': int(eyes),
            'FOV': int(fov),
            'Cumulative Reward Mean': results['cum_reward_mean']
        })

df = pd.DataFrame(rows)

# Adjusting the data to include all combinations and filling missing values
all_fovs = sorted(df['FOV'].unique())
all_eyes = sorted(df['Eyes'].unique())

# Creating a complete grid
heatmap_data_full = pd.DataFrame(index=all_fovs, columns=all_eyes)

# Filling in the data
for _, row in df.iterrows():
    heatmap_data_full.loc[row['FOV'], row['Eyes']] = row['Cumulative Reward Mean']

# Replacing NaNs with a placeholder value for visualization
heatmap_data_full = heatmap_data_full.fillna(-1000)  # Assuming -1000 is the placeholder for untested combinations

plt.figure(figsize=(12, 8))
sns.heatmap(heatmap_data_full.astype(float), annot=True, fmt=".2f", cmap="coolwarm", cbar_kws={'label': 'Cumulative Reward Mean'})
plt.title('Heatmap of Cumulative Reward Mean')
plt.xlabel('Number of Eyes')
plt.ylabel('Field of View (FOV)')
plt.show()
