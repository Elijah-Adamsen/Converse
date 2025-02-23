const API_URL = "link"; 

const authContainer = document.getElementById("auth-container");
const chatContainer = document.getElementById("chat-container");
const userIdInput = document.getElementById("userId");
const registerPhoneInput = document.getElementById("registerPhone");
const registerNameInput = document.getElementById("registerName");
const connectBtn = document.getElementById("connectBtn");
const registerBtn = document.getElementById("registerBtn");
const displayNameInput = document.getElementById("displayName");
const statusMessageInput = document.getElementById("statusMessage");
const saveProfileBtn = document.getElementById("saveProfileBtn");
const groupNameInput = document.getElementById("groupName");
const groupMemberPhoneInput = document.getElementById("groupMemberPhone");
const createGroupBtn = document.getElementById("createGroupBtn");
const addMemberBtn = document.getElementById("addMemberBtn");
const removeMemberBtn = document.getElementById("removeMemberBtn");
const searchInput = document.getElementById("search");
const settingsBtn = document.getElementById("settingsBtn");
const receiverInput = document.getElementById("receiver");
const messageInput = document.getElementById("message");
const sendBtn = document.getElementById("sendBtn");
const messagesContainer = document.getElementById("messages");
const typingIndicator = document.getElementById("typingIndicator");
const fileInput = document.getElementById("fileInput");

let connection;

function showChatContainer() {
    authContainer.style.display = "none";
    chatContainer.style.display = "flex";
}

async function authenticateAndConnect() {
    const phoneNumber = userIdInput?.value.trim();
    if (!phoneNumber) {
        alert("Please enter your phone number.");
        return;
    }

    const token = await authenticate(phoneNumber);
    if (token) {
        localStorage.setItem("jwt_token", token);
        console.log("Stored JWT Token:", token);
        showChatContainer();
        await connectToChat(); // Connect to SignalR only after getting the token
    } else {
        console.error("Token is undefined");
    }
}

async function authenticate(phoneNumber) {
    try {
        console.log("Starting authentication...");
        const response = await fetch(`${API_URL}/api/user/authenticate`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ SenderPhone: phoneNumber })
        });

        const data = await response.json();
        console.log("Full Authentication Response:", data);

        if (!response.ok || !data.token) {
            console.error("Authentication failed:", data);
            alert("Authentication failed. Check console for details.");
            return null;
        }

        console.log("Received JWT Token:", data.token);
        return data.token;

    } catch (err) {
        console.error("Error during authentication:", err);
        alert("Authentication error.");
        return null;
    }
}

async function registerUser() {
    const phoneNumber = registerPhoneInput?.value.trim();
    const name = registerNameInput?.value.trim();

    if (!phoneNumber || !name) {
        alert("Please enter both phone number and name.");
        return;
    }

    try {
        const response = await fetch(`${API_URL}/api/user/register`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ SenderPhone: phoneNumber, Name: name })
        });

        const data = await response.json();
        if (response.ok) {
            alert("Registration successful. Please sign in.");
        } else {
            alert("Registration failed. " + data.message);
        }

    } catch (err) {
        console.error("Error during registration:", err);
        alert("Registration error.");
    }
}

async function connectToChat() {
    const token = localStorage.getItem("jwt_token");

    if (!token) {
        console.error("No JWT token found. Please authenticate first.");
        alert("You must authenticate first.");
        return;
    }

    console.log("Using JWT Token:", token);

    connection = new signalR.HubConnectionBuilder()
        .withUrl(`${API_URL}/chathub`, { 
            accessTokenFactory: () => token
        })
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();

    connection.on("ReceiveMessage", (sender, message) => {
        displayMessage(sender, message);
        console.log(`Message from ${sender}: ${message}`);
    });

    connection.on("UserTyping", (sender) => {
        typingIndicator.style.display = 'block';
        typingIndicator.textContent = `${sender} is typing...`;
    });

    connection.on("UserStoppedTyping", (sender) => {
        typingIndicator.style.display = 'none';
    });

    try {
        await connection.start();
        console.log("Connected to SignalR");
    } catch (err) {
        console.error("Connection failed:", err);
    }
}

document.addEventListener("DOMContentLoaded", () => {
    registerBtn?.addEventListener("click", registerUser);
    connectBtn?.addEventListener("click", authenticateAndConnect); // Only connect after authentication
    sendBtn?.addEventListener("click", sendMessage);
    messageInput?.addEventListener("input", sendTypingNotification);
    saveProfileBtn?.addEventListener("click", saveUserProfile);
    createGroupBtn?.addEventListener("click", createGroup);
    addMemberBtn?.addEventListener("click", addGroupMember);
    removeMemberBtn?.addEventListener("click", removeGroupMember);
    fileInput?.addEventListener("change", sendFile);
    searchInput.addEventListener("input", searchChats);
    settingsBtn.addEventListener("click", toggleDarkMode);
});

async function sendMessage() {
    const receiver = receiverInput?.value.trim();
    const message = messageInput?.value.trim();

    if (!receiver || !message) {
        alert("Please enter both receiver phone and message.");
        return;
    }

    try {
        if (receiver.startsWith("group:")) {
            await connection.invoke("SendMessageToGroup", receiver, message);
        } else {
            await connection.invoke("SendMessageToUser", receiver, message);
        }
        displayMessage("Me", message); // Display the message you sent
        console.log(`Sent message to ${receiver}: ${message}`);
    } catch (err) {
        console.error("Error sending message:", err);
    }
}

async function sendTypingNotification() {
    const receiver = receiverInput?.value.trim();

    if (receiver) {
        await connection.invoke("SendTypingNotification", receiver);
    }
}

async function saveUserProfile() {
    const displayName = displayNameInput?.value.trim();
    const statusMessage = statusMessageInput?.value.trim();

    if (!displayName || !statusMessage) {
        alert("Please enter both display name and status message.");
        return;
    }

    const profile = {
        DisplayName: displayName,
        StatusMessage: statusMessage
    };

    // Save the profile data (you may want to send it to the server)
    console.log("Profile saved:", profile);
    alert("Profile saved.");
}

async function createGroup() {
    const groupName = groupNameInput?.value.trim();

    if (!groupName) {
        alert("Please enter a group name.");
        return;
    }

    try {
        await connection.invoke("CreateGroup", groupName);
        alert("Group created successfully.");
    } catch (err) {
        console.error("Error creating group:", err);
        alert("Error creating group.");
    }
}

async function addGroupMember() {
    const groupName = groupNameInput?.value.trim();
    const memberPhone = groupMemberPhoneInput?.value.trim();

    if (!groupName || !memberPhone) {
        alert("Please enter both group name and member phone.");
        return;
    }

    try {
        await connection.invoke("AddGroupMember", groupName, memberPhone);
        alert("Member added successfully.");
    } catch (err) {
        console.error("Error adding group member:", err);
        alert("Error adding group member.");
    }
}

async function removeGroupMember() {
    const groupName = groupNameInput?.value.trim();
    const memberPhone = groupMemberPhoneInput?.value.trim();

    if (!groupName || !memberPhone) {
        alert("Please enter both group name and member phone.");
        return;
    }

    try {
        await connection.invoke("RemoveGroupMember", groupName, memberPhone);
        alert("Member removed successfully.");
    } catch (err) {
        console.error("Error removing group member:", err);
        alert("Error removing group member.");
    }
}

async function sendFile() {
    const receiver = receiverInput?.value.trim();
    const file = fileInput.files[0];

    if (!receiver || !file) {
        alert("Please select a receiver and a file.");
        return;
    }

    const formData = new FormData();
    formData.append("file", file);
    formData.append("receiver", receiver);

    try {
        const response = await fetch(`${API_URL}/api/chat/upload`, {
            method: "POST",
            body: formData
        });

        const data = await response.json();
        if (response.ok) {
            alert("File sent successfully.");
        } else {
            alert("File upload failed. " + data.message);
        }

    } catch (err) {
        console.error("Error uploading file:", err);
        alert("File upload error.");
    }
}

function displayMessage(sender, message) {
    const messageElement = document.createElement("div");
    messageElement.textContent = `${sender}: ${message}`;
    messagesContainer.appendChild(messageElement);
}

function searchChats() {
    const query = searchInput.value.toLowerCase();
    const chats = document.querySelectorAll("#chatList div");
    chats.forEach(chat => {
        if (chat.textContent.toLowerCase().includes(query)) {
            chat.style.display = "";
        } else {
            chat.style.display = "none";
        }
    });
}

function toggleDarkMode() {
    document.body.classList.toggle("dark-mode");
}