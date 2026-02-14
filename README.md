<a id="readme-top"></a>

<div align="center">

[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![Unlicense License][license-shield]][license-url]
[![Unity](https://img.shields.io/badge/Unity-6000.0.30f1-000000?style=for-the-badge&logo=unity&logoColor=white)](https://unity.com/)
[![C#][CSharp]][CSharp-url]

</div>

<!-- PROJECT LOGO -->
<br />
<div align="center">
  <a href="https://github.com/MurilloYonamine/5th-Semester-Project">
    <img src="Media/unity-engine-logo.png" alt="Logo" width="256" height="256">
  </a>

<h3 align="center">5th Semester Project</h3>

  <p align="center">
    A project made in Unity for University of Senac Santo Amaor
    <br />
    <a href="https://github.com/MurilloYonamine/5th-Semester-Project"><strong>Explore the docs »</strong></a>
    <br />
    <br />
    <a href="https://github.com/MurilloYonamine/5th-Semester-Project">View Demo</a>
    &middot;
    <a href="https://github.com/MurilloYonamine/5th-Semester-Project/issues/new?labels=bug&template=bug-report---.md">Report Bug</a>
    &middot;
    <a href="https://github.com/MurilloYonamine/5th-Semester-Project/issues/new?labels=enhancement&template=feature-request---.md">Request Feature</a>
  </p>
</div>

## Project Architecture

This project uses a simple **Feature-Based Architecture**.

### Folder Structure

```
Assets/
│
├── _Project/   → Project-level files (Readme, tutorial info, misc)
├── _Core/      → Shared systems (EventBus, ServiceLocator, utilities)
├── Features/   → Gameplay features (Player, Enemy, Combat, UI, etc.)
└── _Game/      → Bootstrap, scenes, composition
```

---

### Rules

* No global `Scripts` or `Prefabs` folders.
* Each feature owns its own Scripts, Prefabs, Sprites, etc.
* Features can depend on `_Core`.
* Features should not directly depend on other features.
* `_Game` references Core and Features and handles setup.

---

### Feature Template

```
FeatureName/
│
├── Scripts/
├── Prefabs/
├── Models/
├── Sprites/
├── Animations/
├── Audio/
└── FeatureName.asmdef
```

### Layer Style

<a href="https://github.com/MurilloYonamine/5th-Semester-Project">
  <img src="Media/Diagrams/architecture.drawio.png" alt="Logo">
</a>

## Credits

CI/CD powered by [GameCI](https://game.ci/) and GitHub Actions.


<p align="right">(<a href="#readme-top">back to top</a>)</p>


<!-- MARKDOWN LINKS & IMAGES -->
[contributors-shield]: https://img.shields.io/github/contributors/MurilloYonamine/5th-Semester-Project.svg?style=for-the-badge
[contributors-url]: https://github.com/MurilloYonamine/5th-Semester-Project/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/MurilloYonamine/5th-Semester-Project.svg?style=for-the-badge
[forks-url]: https://github.com/MurilloYonamine/5th-Semester-Project/network/members
[stars-shield]: https://img.shields.io/github/stars/MurilloYonamine/5th-Semester-Project.svg?style=for-the-badge
[stars-url]: https://github.com/MurilloYonamine/5th-Semester-Project/stargazers
[issues-shield]: https://img.shields.io/github/issues/MurilloYonamine/5th-Semester-Project.svg?style=for-the-badge
[issues-url]: https://github.com/MurilloYonamine/5th-Semester-Project/issues
[license-shield]: https://img.shields.io/github/license/MurilloYonamine/5th-Semester-Project.svg?style=for-the-badge
[license-url]: https://github.com/MurilloYonamine/5th-Semester-Project/blob/master/LICENSE.txt
[Unity]: https://img.shields.io/badge/Unity-000000?style=for-the-badge&logo=unity&logoColor=white
[Unity-url]: https://unity.com/
[CSharp]: https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=dotnet&logoColor=white
[CSharp-url]: https://docs.microsoft.com/en-us/dotnet/csharp/