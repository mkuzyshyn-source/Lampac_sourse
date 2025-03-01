// This script helps upload your project to GitHub
const fs = require('fs');
const path = require('path');
const https = require('https');

// Configuration - replace these values
const GITHUB_TOKEN = 'Y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'; // Personal access token
const GITHUB_USERNAME = 'mkuzyshyn-source';
const REPO_NAME = 'Lampac_sourse';

// Function to create a new repository
function createRepo() {
  return new Promise((resolve, reject) => {
    const data = JSON.stringify({
      name: REPO_NAME,
      description: 'Repository created from WebContainer',
      private: false
    });

    const options = {
      hostname: 'api.github.com',
      path: '/user/repos',
      method: 'POST',
      headers: {
        'User-Agent': 'Node.js',
        'Content-Type': 'application/json',
        'Authorization': `token ${GITHUB_TOKEN}`,
        'Content-Length': data.length
      }
    };

    const req = https.request(options, (res) => {
      let responseData = '';
      
      res.on('data', (chunk) => {
        responseData += chunk;
      });
      
      res.on('end', () => {
        if (res.statusCode >= 200 && res.statusCode < 300) {
          console.log('Repository created successfully!');
          resolve(JSON.parse(responseData));
        } else {
          console.error(`Failed to create repository: ${res.statusCode}`);
          console.error(responseData);
          reject(new Error(`Failed to create repository: ${res.statusCode}`));
        }
      });
    });
    
    req.on('error', (error) => {
      console.error('Error creating repository:', error);
      reject(error);
    });
    
    req.write(data);
    req.end();
  });
}

// Function to upload a file to GitHub
function uploadFile(filePath, repoPath) {
  return new Promise((resolve, reject) => {
    fs.readFile(filePath, 'utf8', (err, content) => {
      if (err) {
        console.error(`Error reading file ${filePath}:`, err);
        reject(err);
        return;
      }

      const relativePath = path.relative(process.cwd(), filePath);
      const githubPath = repoPath ? `${repoPath}/${relativePath}` : relativePath;
      
      const data = JSON.stringify({
        message: `Add ${relativePath}`,
        content: Buffer.from(content).toString('base64')
      });

      const options = {
        hostname: 'api.github.com',
        path: `/repos/${GITHUB_USERNAME}/${REPO_NAME}/contents/${githubPath}`,
        method: 'PUT',
        headers: {
          'User-Agent': 'Node.js',
          'Content-Type': 'application/json',
          'Authorization': `token ${GITHUB_TOKEN}`,
          'Content-Length': data.length
        }
      };

      const req = https.request(options, (res) => {
        let responseData = '';
        
        res.on('data', (chunk) => {
          responseData += chunk;
        });
        
        res.on('end', () => {
          if (res.statusCode >= 200 && res.statusCode < 300) {
            console.log(`Uploaded ${filePath} successfully!`);
            resolve();
          } else {
            console.error(`Failed to upload ${filePath}: ${res.statusCode}`);
            console.error(responseData);
            reject(new Error(`Failed to upload ${filePath}: ${res.statusCode}`));
          }
        });
      });
      
      req.on('error', (error) => {
        console.error(`Error uploading ${filePath}:`, error);
        reject(error);
      });
      
      req.write(data);
      req.end();
    });
  });
}

// Function to recursively get all files in a directory
function getAllFiles(dirPath, arrayOfFiles = []) {
  const files = fs.readdirSync(dirPath);

  files.forEach(file => {
    if (fs.statSync(dirPath + "/" + file).isDirectory()) {
      arrayOfFiles = getAllFiles(dirPath + "/" + file, arrayOfFiles);
    } else {
      arrayOfFiles.push(path.join(dirPath, "/", file));
    }
  });

  return arrayOfFiles;
}

// Main function to upload the project
async function uploadProject() {
  try {
    // Create the repository
    await createRepo();
    
    // Get all files in the project
    const files = getAllFiles('.');
    
    // Filter out node_modules, .git, and other files you don't want to upload
    const filesToUpload = files.filter(file => {
      return !file.includes('node_modules') && 
             !file.includes('.git') && 
             !file.includes('github-upload.js') &&
             !file.endsWith('.log');
    });
    
    // Upload each file
    for (const file of filesToUpload) {
      await uploadFile(file);
      console.log(`Uploaded ${file}`);
    }
    
    console.log('Project uploaded successfully!');
    console.log(`Visit your repository at: https://github.com/${GITHUB_USERNAME}/${REPO_NAME}`);
  } catch (error) {
    console.error('Error uploading project:', error);
  }
}

// Check if configuration is set
if (GITHUB_TOKEN === 'YOUR_GITHUB_TOKEN' || 
    GITHUB_USERNAME === 'YOUR_GITHUB_USERNAME' || 
    REPO_NAME === 'YOUR_REPO_NAME') {
  console.log('Please update the configuration in the script with your GitHub information:');
  console.log('1. GITHUB_TOKEN: Create a personal access token at https://github.com/settings/tokens');
  console.log('   with "repo" permissions');
  console.log('2. GITHUB_USERNAME: Your GitHub username');
  console.log('3. REPO_NAME: The name for your new repository');
} else {
  uploadProject();
}
