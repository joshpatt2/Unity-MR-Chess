# GitHub Setup Commands

## Installing GitHub CLI (Recommended)

### Option 1: Install via Homebrew (if you have it)
```bash
brew install gh
```

### Option 2: Download from GitHub
1. Go to https://cli.github.com/
2. Download the macOS installer
3. Install and restart terminal

### Option 3: Manual Installation
```bash
# Download and install GitHub CLI
curl -fsSL https://cli.github.com/packages/githubcli-archive-keyring.gpg | sudo dd of=/usr/share/keyrings/githubcli-archive-keyring.gpg
echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/githubcli-archive-keyring.gpg] https://cli.github.com/packages stable main" | sudo tee /etc/apt/sources.list.d/github-cli.list > /dev/null
sudo apt update
sudo apt install gh
```

## After Installing GitHub CLI

### Quick Repository Creation with GitHub CLI:
```bash
cd /Users/joshuapatterson/Chess-AI

# Login to GitHub
gh auth login

# Create repository
gh repo create Unity-MR-Chess --public --description "Mixed Reality Chess game for Meta Quest using Unity and OpenXR"

# Push code
git branch -M main
git push -u origin main
```

## Manual Setup (if GitHub CLI not available)

After creating a repository on GitHub.com, run these commands:

## Replace YOUR_USERNAME with your actual GitHub username
```bash
cd /Users/joshuapatterson/Chess-AI

# Add GitHub as remote origin
git remote add origin https://github.com/YOUR_USERNAME/Unity-MR-Chess.git

# Push to GitHub
git branch -M main
git push -u origin main
```

## Alternative with SSH (if you have SSH keys set up):
```bash
git remote add origin git@github.com:YOUR_USERNAME/Unity-MR-Chess.git
git branch -M main  
git push -u origin main
```

## Quick GitHub Repository Creation:
1. Go to https://github.com/new
2. Repository name: Unity-MR-Chess
3. Description: "Mixed Reality Chess game for Meta Quest using Unity and OpenXR"
4. Public repository
5. Don't add README, .gitignore, or license (we have them)
6. Click "Create repository"
7. Copy the repository URL
8. Run the commands above with your URL
