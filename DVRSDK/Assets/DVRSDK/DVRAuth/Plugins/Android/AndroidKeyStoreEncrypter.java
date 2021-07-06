package com.dmm.vrlab.hena.security;

import android.security.keystore.KeyGenParameterSpec;
import android.security.keystore.KeyProperties;
import android.util.Base64;
import android.util.Log;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.math.BigInteger;
import java.security.Key;
import java.security.KeyPairGenerator;
import java.security.KeyStore;
import java.security.PublicKey;
import java.security.spec.MGF1ParameterSpec;
import java.util.UUID;

import javax.crypto.Cipher;
import javax.crypto.CipherInputStream;
import javax.crypto.CipherOutputStream;
import javax.crypto.KeyGenerator;
import javax.crypto.SecretKey;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.OAEPParameterSpec;
import javax.crypto.spec.PSource;
import javax.security.auth.x500.X500Principal;

public class AndroidKeyStoreEncrypter {
    private static final String TAG = "HenaAndroidKeyStoreEncrypter";

    private static final String KEY_PROVIDER = "AndroidKeyStore";
    private static final String KEY_ALIAS_RSA = "HenaAndroidKeyStoreEncrypterRSA";
    private static final String KEY_ALIAS_AES = "HenaAndroidKeyStoreEncrypterAES";
    private static final String RSA_ALGORITHM = "RSA/ECB/OAEPWithSHA-256AndMGF1Padding";
    private static final String AES_ALGORITHM = "AES/CBC/PKCS7Padding";
    private static final int IVKEY_SIZE = 256;

    protected void initKeyStoreRSA() {
        try {
            KeyStore keyStore = KeyStore.getInstance(KEY_PROVIDER);
            keyStore.load(null);

            if (!keyStore.containsAlias(KEY_ALIAS_RSA)) {
                KeyPairGenerator keyPairGenerator = KeyPairGenerator.getInstance(
                        KeyProperties.KEY_ALGORITHM_RSA, KEY_PROVIDER);
                keyPairGenerator.initialize(
                        new KeyGenParameterSpec.Builder(
                                KEY_ALIAS_RSA,
                                KeyProperties.PURPOSE_DECRYPT | KeyProperties.PURPOSE_ENCRYPT)
                                .setDigests(KeyProperties.DIGEST_SHA256, KeyProperties.DIGEST_SHA512)
                                .setEncryptionPaddings(KeyProperties.ENCRYPTION_PADDING_RSA_OAEP)
                                .build());
                keyPairGenerator.generateKeyPair();
            }
        } catch (Exception e) {
            Log.e(TAG, e.toString());
        }
    }

    protected Key getPrivateKey() {
        try {
            initKeyStoreRSA();

            KeyStore keyStore = KeyStore.getInstance(KEY_PROVIDER);
            keyStore.load(null);

            return keyStore.getKey(KEY_ALIAS_RSA, null);
        } catch (Exception e) {
            Log.e(TAG, e.toString());
        }

        return null;
    }

    protected PublicKey getPublicKey() {
        try {
            initKeyStoreRSA();

            KeyStore keyStore = KeyStore.getInstance(KEY_PROVIDER);
            keyStore.load(null);

            return keyStore.getCertificate(KEY_ALIAS_RSA).getPublicKey();
        } catch (Exception e) {
            Log.e(TAG, e.toString());
        }

        return null;
    }

    protected byte[] encryptRSA(String plainString) {
        try {
            Key key = getPublicKey();
            OAEPParameterSpec sp = new OAEPParameterSpec("SHA-256", "MGF1", new MGF1ParameterSpec("SHA-1"), PSource.PSpecified.DEFAULT);
            Cipher cipher = Cipher.getInstance(RSA_ALGORITHM);
            cipher.init(Cipher.ENCRYPT_MODE, key, sp);
            return cipher.doFinal(plainString.getBytes());
        } catch (Exception e) {
            Log.e(TAG, e.toString());
        }

        return null;
    }

    protected String decryptRSA(byte[] encryptedData) {
        try {
            Key key = getPrivateKey();
            OAEPParameterSpec sp = new OAEPParameterSpec("SHA-256", "MGF1", new MGF1ParameterSpec("SHA-1"), PSource.PSpecified.DEFAULT);
            Cipher cipher = Cipher.getInstance(RSA_ALGORITHM);
            cipher.init(Cipher.DECRYPT_MODE, key, sp);
            byte[] decryptedBytes = cipher.doFinal(encryptedData);

            return new String(decryptedBytes);
        } catch (Exception e) {
            Log.e(TAG, e.toString());
        }

        return null;
    }


    protected void initKeyStoreAES() {
        try {
            KeyStore keyStore = KeyStore.getInstance(KEY_PROVIDER);
            keyStore.load(null);

            if (!keyStore.containsAlias(KEY_ALIAS_AES)) {
                KeyGenerator keyGenerator = KeyGenerator.getInstance(KeyProperties.KEY_ALGORITHM_AES,"AndroidKeyStore");
                keyGenerator.init(new KeyGenParameterSpec.Builder(KEY_ALIAS_AES,KeyProperties.PURPOSE_ENCRYPT|KeyProperties.PURPOSE_DECRYPT)
                        .setCertificateSubject(new X500Principal("CN="+KEY_ALIAS_AES))
                        .setCertificateSerialNumber(BigInteger.ONE)
                        .setBlockModes(KeyProperties.BLOCK_MODE_CBC)
                        .setEncryptionPaddings(KeyProperties.ENCRYPTION_PADDING_PKCS7)
                        .setRandomizedEncryptionRequired(false)
                        .build());
                keyGenerator.generateKey();
            }
        } catch (Exception e) {
            Log.e(TAG, e.toString());
        }
    }

    protected SecretKey getSecretKey() {
        try {
            initKeyStoreAES();

            KeyStore keyStore = KeyStore.getInstance(KEY_PROVIDER);
            keyStore.load(null);

            return (SecretKey)keyStore.getKey(KEY_ALIAS_AES, null);
        } catch (Exception e) {
            Log.e(TAG, e.toString());
        }

        return null;
    }

    protected String CreateNewAESIVKey(){
        return UUID.randomUUID().toString().substring(0, 16);
    }

    public String encrypt(String plainString) {
        try {
            SecretKey secretKey = getSecretKey();

            String ivKey = CreateNewAESIVKey();

            Cipher cipher = Cipher.getInstance(AES_ALGORITHM);
            IvParameterSpec ivParameterSpec = new IvParameterSpec(ivKey.getBytes());
            cipher.init(Cipher.ENCRYPT_MODE,secretKey,ivParameterSpec);

            ByteArrayOutputStream outputStream = new ByteArrayOutputStream();
            outputStream.write(encryptRSA(ivKey));
            CipherOutputStream cipherOutputStream = new CipherOutputStream(outputStream,cipher);
            cipherOutputStream.write(plainString.getBytes("UTF-8"));
            cipherOutputStream.close();
            return Base64.encodeToString(outputStream.toByteArray(),Base64.NO_WRAP);
        } catch (Exception e) {
            Log.e(TAG, e.toString());
        }

        return null;
    }

    public String decrypt(String encryptText) {
        try {
            SecretKey secretKey = getSecretKey();

            Cipher cipher = Cipher.getInstance(AES_ALGORITHM);

            byte[] encryptData = Base64.decode(encryptText,Base64.NO_WRAP);

            ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();
            byteArrayOutputStream.write(encryptData, 0, IVKEY_SIZE);
            String ivKey = decryptRSA(byteArrayOutputStream.toByteArray());
            IvParameterSpec ivParameterSpec = new IvParameterSpec(ivKey.getBytes());
            byteArrayOutputStream.close();

            cipher.init(Cipher.DECRYPT_MODE,secretKey,ivParameterSpec);

            CipherInputStream cipherInputStream = new CipherInputStream(new ByteArrayInputStream(encryptData, IVKEY_SIZE, encryptData.length - IVKEY_SIZE) ,cipher);
            byteArrayOutputStream = new ByteArrayOutputStream();
            int buffer;
            while ((buffer = cipherInputStream.read())!= -1) {
                byteArrayOutputStream.write(buffer);
            }
            byteArrayOutputStream.close();
            return byteArrayOutputStream.toString("UTF-8");
        } catch (Exception e) {
            Log.e(TAG, e.toString());
        }

        return null;
    }
}
