# Mice

![Platform](https://img.shields.io/badge/platform-Rhino6%20%7C%20Grasshopper-orange)
![License](https://img.shields.io/github/license/hrntsm/Mice)
![Release](https://img.shields.io/github/v/release/hrntsm/Mice)
![Download](https://img.shields.io/github/downloads/hrntsm/Mice/total)

Simple structural analysis components for Grasshopper. Response analysis for 1 DOF and stress analysis for simple beams are available.

Response analysis is an experimental feature.

## Install

1. Download Mice.gha file from [food4rhino](https://www.food4rhino.com/app/Mice) or [release page](https://github.com/hrntsm/Mice/releases)
2. In Grasshopper, choose File > Special Folders > Components folder. Save the gha file there.  
3. Right-click the file > Properties > make sure there is no "blocked" text
4. Restart Rhino and Grasshopper

## Stress Analysis for simple beams

This feature provides a simple stress analysis and cross-sectional verification of the beam. It includes the following

+ Any Moment
  + Calculate for any given direct input moment
+ Centralized Load
  + Calculation for centralized load
+ Trapezoid Load
  + Calculation for Trapezoidal Distribution Load
+ Cantilever Point Load
  + Calculation for cantilevered beam tip load
+ Box Shape
  + Calculate the cross-section parameter of a box shape section
+ H Shape
  + Calculate the cross-section parameter of a I shape section
+ L Shape
  + Calculate the cross-section parameter of a L shape section
+ Moment View
  + Display the moment of analysis on Rhino

## Response Analysis

This feature uses the Newmark β method to analyze the response of a single DOF mass system. It includes the following

+ Calc T
  + A peculiar period and a natural frequency and a peculiar angular frequency are calculated by mass and rigidity
+ MTtoK  
  + Calculate rigidity from mass and a peculiar period  
+ KTtoM  
  + Calculate mass from stiffness and a peculiar period  
+ MakeSinWave  
  + Make a sin wave as an input wave  
+ 1DOF Response Analysis 
  + For input parameter, calculate 1dof response analysis in Newmarkβ method and output result  
+ Response Spectrum
  + Calculate Response Spectrum of given wave date
+ ModelView  
  + Output a model on Rhino  
+ ResultView
  + Move the model that output on Rhino depending on a result  

## Contact information

[![Twitter](https://img.shields.io/twitter/follow/hiron_rgkr?style=social)](https://twitter.com/hiron_rgkr)
+ HomePage : [https://hrntsm.github.io/](https://hrntsm.github.io/)
+ blog : [https://rgkr-memo.blogspot.com/](https://rgkr-memo.blogspot.com/)
+ Mail : support(at)hrntsm.com
  + change (at) to @
  
## License

HoaryFox is licensed under the [MIT](https://github.com/hrntsm/Mice/blob/master/LICENSE) license.  
Copyright© 2020, hrntsm
