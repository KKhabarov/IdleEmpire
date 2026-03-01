# Idle Empire — Economy Balance Document

## Overview

This document describes the tuning parameters for Idle Empire's 10-business progression,
15 upgrades, 10 managers, and prestige system. Use it as a reference when adjusting values
in the ScriptableObject `.asset` files.

---

## 1. Business Stats & Income/Second at Level 1

| # | Business Name    | Base Cost         | Base Income | Cycle (s) | Cost ×  | Income/sec (lvl 1) | ROI (s)   |
|---|-----------------|-------------------|-------------|-----------|---------|-------------------|-----------|
| 1 | Lemonade Stand   | 4                 | 1           | 1.0       | ×1.07   | 1.00              | 4         |
| 2 | Newspaper Route  | 60                | 3           | 3.0       | ×1.15   | 1.00              | 60        |
| 3 | Car Wash         | 720               | 8           | 6.0       | ×1.14   | 1.33              | 540       |
| 4 | Pizza Delivery   | 8,640             | 20          | 12.0      | ×1.13   | 1.67              | 5,184     |
| 5 | Donut Shop       | 103,680           | 50          | 24.0      | ×1.12   | 2.08              | 49,846    |
| 6 | Shrimp Boat      | 1,244,160         | 120         | 48.0      | ×1.11   | 2.50              | 497,664   |
| 7 | Hockey Team      | 14,929,920        | 300         | 96.0      | ×1.10   | 3.13              | 4,774,374 |
| 8 | Movie Studio     | 179,159,040       | 750         | 192.0     | ×1.09   | 3.91              | ~45.8M    |
| 9 | Bank             | 2,149,908,480     | 1,800       | 384.0     | ×1.08   | 4.69              | ~458M     |
|10 | Oil Company      | 25,798,901,760    | 4,500       | 768.0     | ×1.07   | 5.86              | ~4.40B    |

**ROI** = Base Cost ÷ Income/sec at level 1.  
Each successive business is significantly more expensive but also generates income faster per
dollar as the player scales up — creating a satisfying "just one more upgrade" loop.

---

## 2. ROI Analysis

- **Lemonade Stand**: ROI = 4 s — effectively free; every player starts here.
- **Newspaper Route**: ROI = 60 s — unlocks naturally after ~1 minute of play.
- **Car Wash → Pizza Delivery**: ROI scales from ~9 min to ~1.5 h; manager investment
  becomes attractive during this range.
- **Donut Shop → Shrimp Boat**: ROI in hours; offline earnings start to matter.
- **Hockey Team → Oil Company**: ROI measured in days of active play; prestige resets
  become the primary progression lever.

---

## 3. Upgrade Impact Analysis

Each upgrade doubles (×2) or triples (×3) the income of its target business.

| # | Upgrade              | Cost          | ×   | Target Business   |
|---|---------------------|---------------|-----|-------------------|
| 1 | Bigger Cups          | 100           | ×2  | Lemonade Stand    |
| 2 | Electric Bike        | 1,000         | ×2  | Newspaper Route   |
| 3 | Power Washer         | 10,000        | ×2  | Car Wash          |
| 4 | Turbo Oven           | 100,000       | ×2  | Pizza Delivery    |
| 5 | Secret Recipe        | 500,000       | ×2  | Donut Shop        |
| 6 | Sonar Equipment      | 2,500,000     | ×2  | Shrimp Boat       |
| 7 | Star Player          | 10,000,000    | ×2  | Hockey Team       |
| 8 | CGI Effects          | 50,000,000    | ×2  | Movie Studio      |
| 9 | AI Trading           | 250,000,000   | ×2  | Bank              |
|10 | Deep Drilling        | 1,000,000,000 | ×2  | Oil Company       |
|11 | Organic Lemons       | 50,000        | ×3  | Lemonade Stand    |
|12 | Drone Delivery       | 5,000,000     | ×3  | Pizza Delivery    |
|13 | Franchise Deal       | 25,000,000    | ×3  | Donut Shop        |
|14 | Streaming Platform   | 500,000,000   | ×3  | Movie Studio      |
|15 | Central Bank         | 5,000,000,000 | ×3  | Bank              |

**Cumulative multiplier** for businesses with two upgrades:
- Lemonade Stand: ×2 × ×3 = **×6**
- Pizza Delivery: ×2 × ×3 = **×6**
- Donut Shop: ×2 × ×3 = **×6**
- Movie Studio: ×2 × ×3 = **×6**
- Bank: ×2 × ×3 = **×6**

---

## 4. Manager Unlock Progression

Managers automate a business so it collects income without player interaction.
Each manager's cost is roughly 250× the business base cost.

| # | Manager                | Cost              | Target Business  |
|---|----------------------|-------------------|------------------|
| 1 | Timmy the Kid          | 1,000             | Lemonade Stand   |
| 2 | Paperboy Pete          | 15,000            | Newspaper Route  |
| 3 | Sudsy Sam              | 150,000           | Car Wash         |
| 4 | Pizza Paul             | 1,500,000         | Pizza Delivery   |
| 5 | Donut Donna            | 15,000,000        | Donut Shop       |
| 6 | Captain Shrimp         | 100,000,000       | Shrimp Boat      |
| 7 | Coach Gretzky          | 750,000,000       | Hockey Team      |
| 8 | Director Spielburger   | 5,000,000,000     | Movie Studio     |
| 9 | Banker Burns           | 25,000,000,000    | Bank             |
|10 | Oil Baron Rex          | 150,000,000,000   | Oil Company      |

Hiring a manager is a multiplier on player attention, not income — it frees the player
to focus on the next business while earlier ones run passively.

---

## 5. Prestige Multiplier Scaling

The prestige multiplier (`prestigeMultiplier`) is stored in `SaveData` and applied globally
to all business income calculations. Suggested scaling:

| Prestige Level | Suggested Multiplier | Unlock Condition |
|---------------|----------------------|-----------------|
| 0 (base)      | ×1.0                 | Start           |
| 1             | ×2.0                 | Reach ~$1B      |
| 2             | ×4.0                 | Reach ~$100B    |
| 3             | ×8.0                 | Reach ~$10T     |
| 4             | ×16.0                | Reach ~$1Qa     |

Each prestige resets money and business levels but retains the multiplier, creating
exponentially faster subsequent runs.

---

## 6. Economy Tuning Tips

1. **Adjust `CostMultiplier`** to control how fast each business becomes expensive.
   Lower values (e.g., 1.07) make late-game businesses scale more slowly.

2. **`CycleDuration`** controls the "feel" of a business. Short cycles (1 s) feel active
   and clicky; long cycles (768 s) feel like long-term investments.

3. **Income/sec parity**: The table shows income/sec increasing with business index,
   ensuring that spending money on later businesses is always worthwhile.

4. **Upgrade timing**: The first upgrade for each business is priced at roughly 25×
   the business base cost, making it achievable soon after purchasing that business.

5. **Manager timing**: Managers are priced at roughly 250× the business base cost,
   so players who invest in a business will eventually want to automate it.

6. **Offline earnings** (`OfflineCalculator`) are capped to prevent abuse. A cap of
   4–8 hours of production at the current income rate is typical for idle games.

7. **Prestige threshold**: Set the prestige unlock condition high enough that players
   have automated most businesses before resetting. This keeps the loop satisfying.
